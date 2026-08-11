# 提案:篝火"钓鱼"——拆解 / 重组拼接卡

> 本提案供另一个 agent 直接实施。所有改动均在 `D:\lg\else\mod\` 下。
> 只新增 2 个 .cs 文件 + 1 张本地化表,改 2 个现有 .cs 文件 + 1 张本地化表,无游戏侧改动。

---

## 0. 一句话

给篝火新增"钓鱼"选项:把一张完整拼卡拆成"费用牌(留在卡组)+ 效果牌(进入可自由拿取的暂存奖励池)",再把池里的效果牌拼回任意能拼上的费用卡。拼回顺序 = 拼接顺序,从而实现对效果的**重新组合、重排**。

---

## 1. 需求

1. 玩家可**拆解**拼接卡:一张完整拼卡 → 对应费用牌 + 效果牌。费用牌仍留在卡组,效果牌进入奖励(奖励非 n 选 1,允许玩家随意拿走)。
2. 玩家可从奖励中把**自己刚拆下来的牌**重新拼回去。
3. 意义:允许重新组合、排列效果牌顺序,提升牌效。
4. 前提:拼接卡必须记录自己由哪几张效果牌拼接而来(没有则为 null)。

---

## 2. 关键结论(决定工作量的现状分析)

> 以下均来自反编译源码 + 现有 mod 代码,已逐条验证。

### 2.1 "记录来源效果卡"——**现有架构已满足,无需新增持久化字段**

- 每拼一张效果卡,`SpliceController.AttachEffect` 会给宿主卡挂一个 `EffectAttachmentModifier`(BaseLib 的 `CardModifier` 子类),实例上存:
  - `EffectCardId`(字符串,即"由哪张效果牌拼成")
  - `JoinIndex`(加入顺序)
- `StoreSaveData/LoadSaveData` 已把这两个字段写入 `ModifierSave`(BaseLib 自动随卡片存档),**跨存档保留**。
- `SpliceController.AttachedEffects(card)` 返回**按顺序**的全部效果卡;没有拼接 = 卡上没有该 modifier = 干净费用卡。
- 因此"拼卡由哪几张效果牌拼成、无则为 null"这个数据**已经存在**。本提案只补一个只读便捷属性 `SourceEffectCards`(见 §4.4),不新增任何存储。

### 2.2 拆解/拼接 API 齐全

- 拆:`CardModifier.RemoveModifier(card, modifier)`(public static)+ `CardModel.RemoveKeyword(keyword)`(public)。
- 拼:`SpliceController.AttachEffect(target, effect)`(已有,追加顺序 = 拼接顺序)。
- 卡组增删:`CardPileCmd.RemoveFromDeck / Add`,本方案**不需要**动卡组(拆解是"原地卸效果",费用卡实例留在卡组,升级等状态不丢)。

### 2.3 篝火扩展点 = `RestSiteOption`(public 抽象)+ Harmony 注入

- `RestSiteOption` 是 public 抽象类:`abstract string OptionId` + `abstract Task<bool> OnSelect()` 必须实现;`Description` 可 override。
- 每次进篝火,`RestSiteSynchronizer` 调 `RestSiteOption.Generate(Player)`(public static 工厂)生成选项列表。
- mod 用 **Harmony Postfix 在 `Generate` 上追加我们的选项**(与现有 `RewardInjectionPatch` patch `RewardsSet.GenerateRewardsFor` 是同一模式),**无需 ModelDb 注册、无需玩家持有特定牌/遗物**。
- 备选方案(不采用):`ModHelper.SubscribeForRunStateHooks` + 自定义 `AbstractModel` 重写 `TryModifyRestSiteOptions`。**不可用**——`AbstractModel` 有抽象成员 `ShouldReceiveCombatHooks` 且构造函数走 ModelDb 查询,裸 new 的非注册模型不可靠。
- BaseLib 提供 `CustomRestSiteOption : RestSiteOption`(可自定义图标路径),直接继承它。

### 2.4 非战斗场景选牌屏

- `CardSelectCmd.FromDeckGeneric(Player, prefs, filter)` —— 弹卡组选择屏,**无需** `PlayerChoiceContext`。拆解选拼卡、重组选目标费用卡都用它。
- `CardSelectCmd.FromSimpleGrid(PlayerChoiceContext, cards, player, prefs)` —— 弹网格多选屏,需要 context。从暂存池选效果卡用它;单机传 `new BlockingPlayerChoiceContext()`(no-op 上下文,AttackCommand 内部回退同款)。
- `SpliceReward` 已示范 `CardSelectCmd.FromDeckGeneric` + `CardSelectorPrefs(prompt, 1) { Cancelable = true }` 的用法,照抄即可。

### 2.5 暂存池是会话内的(重要设计决策)

- 拆下来的效果卡放进 `OnSelect` 内的局部列表 `staged`,作为"奖励池"。
- 钓鱼结束时池里**剩余的效果卡丢弃**(放生)。这是"随意拿走,不拿就放"的语义。
- 池不跨存档/不跨篝火持久化;若想"奖励长期保留"需自建 `Reward` 子类 + 序列化(列为未来工作,见 §6-7)。

---

## 3. 文件改动清单

| # | 文件 | 动作 |
|---|------|------|
| 1 | `src/PartSmith/PartSmithCode/RestSite/FishingRestSiteOption.cs` | **新增** — 钓鱼选项本体(OnSelect 主逻辑) |
| 2 | `src/PartSmith/PartSmithCode/Patches/RestSiteOptionInjectionPatch.cs` | **新增** — Harmony Postfix 注入选项 |
| 3 | `src/PartSmith/PartSmithCode/Cards/Splicing/SpliceController.cs` | **修改** — 加 `IsSpliced` + `DetachAllEffects` |
| 4 | `src/PartSmith/PartSmithCode/Cards/Base/CostCardModelBase.cs` | **修改** — 加 `SourceEffectCards` 只读属性 |
| 5 | `src/PartSmith/PartSmith/localization/eng/rest_site_ui.json` | **新增** — 选项标题/描述 |
| 6 | `src/PartSmith/PartSmith/localization/eng/card_selection.json` | **修改** — 追加钓鱼提示/吐司文本 |
| 7 | `src/PartSmith/PartSmith/images/rest_site/option_partsmith_fish.png` | **可选** — 钓鱼图标 |
| 8 | `src/PartSmith/PartSmith/localization/zhs/…` | **可选** — 中文文案 |

---

## 4. 改动细节(完整可粘贴代码)

### 4.1 `FishingRestSiteOption.cs`(新增)

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Cards.Splicing;
using PartSmith.PartSmithCode.Rewards;

namespace PartSmith.PartSmithCode.RestSite;

/// <summary>
/// 篝火"钓鱼":把完整拼卡拆成"费用牌(留卡组)+ 效果牌(进暂存奖励池)",
/// 再把池里的效果牌自由拼回(顺序 = 拼接顺序,实现重新组合/重排)。
///
/// 交互流程:
/// 1. 拆解(Phase A):循环弹出"卡组中可拆拼卡"选择,每选一张就把它的全部效果卸进池里;
///    ESC 进入重组。没有可拆的拼卡也直接进入重组。
/// 2. 重组(Phase B):循环"从池里选一张效果卡 → 从卡组选一张能拼上的费用卡"拼接;
///    选目标时 ESC 等于放回继续挑,在选效果卡时 ESC 才结束。
/// 3. 什么都没做 → OnSelect 返回 false(选项不消耗,篝火可继续选治疗/锻造);
///    做了任何动作 → 返回 true(消耗本次篝火);结束时池里剩余效果卡丢弃(放生)。
///
/// 多人未实现:只处理本地玩家(与 SpliceReward 一致,远端直接照旧不处理)。
/// </summary>
public class FishingRestSiteOption : CustomRestSiteOption
{
    private static readonly LocString DisassemblePrompt = new("card_selection", "PARTSMITH_FISH_DISASSEMBLE_PROMPT");
    private static readonly LocString SpliceEffectPrompt = new("card_selection", "PARTSMITH_FISH_SPLICE_EFFECT_PROMPT");
    private static readonly LocString SpliceTargetPrompt = new("card_selection", "PARTSMITH_FISH_SPLICE_TARGET_PROMPT");
    private static readonly LocString NothingToDisassemble = new("card_selection", "PARTSMITH_FISH_NO_SPLICED_CARDS");
    private static readonly LocString CaughtToast = new("card_selection", "PARTSMITH_FISH_CAUGHT");

    public override string OptionId => "PARTSMITH_FISH";

    /// <summary>
    /// 自定义图标(整条 res:// 路径)。默认返回 null 走游戏默认路径
    /// (ui/rest_site/option_partsmith_fish.png,不存在 → 按钮可能无图标,首测确认不崩)。
    /// 想显示图标:放一张 PNG 到 src/PartSmith/PartSmith/images/rest_site/option_partsmith_fish.png,
    /// 并改成返回 "res://PartSmith/images/rest_site/option_partsmith_fish.png"。
    /// </summary>
    public override string? CustomIconPath => null;

    public FishingRestSiteOption(Player owner) : base(owner) { }

    public override async Task<bool> OnSelect()
    {
        var staged = new List<CardModel>();
        bool anythingChanged = false;

        // Phase A —— 拆解(可连续拆多张;ESC 进入重组)。
        while (true)
        {
            if (!PileType.Deck.GetPile(Owner).Cards.Any(SpliceController.IsSpliced))
            {
                if (!anythingChanged)
                {
                    await SpliceToast.Show(NothingToDisassemble.GetFormattedText());
                }
                break;
            }

            var pick = (await CardSelectCmd.FromDeckGeneric(
                Owner,
                new CardSelectorPrefs(DisassemblePrompt, 1) { Cancelable = true },
                SpliceController.IsSpliced)).ToList();
            if (pick.Count == 0)
            {
                break; // ESC → 进入重组
            }

            var effects = SpliceController.DetachAllEffects(pick[0]);
            staged.AddRange(effects);
            anythingChanged = true;

            var toast = new LocString("card_selection", "PARTSMITH_FISH_CAUGHT");
            toast.AddObj("count", effects.Count);
            await SpliceToast.Show(toast.GetFormattedText());
        }

        // Phase B —— 重组(把池里效果卡自由拼回;顺序 = 拼接顺序;选效果卡时 ESC 结束)。
        while (staged.Count > 0)
        {
            var effPick = (await CardSelectCmd.FromSimpleGrid(
                new BlockingPlayerChoiceContext(),
                staged,
                Owner,
                new CardSelectorPrefs(SpliceEffectPrompt, 1))).ToList();
            if (effPick.Count == 0)
            {
                break; // ESC → 结束,池里剩余效果卡丢弃
            }

            var effect = (EffectCardModelBase)effPick[0];
            var target = (await CardSelectCmd.FromDeckGeneric(
                Owner,
                new CardSelectorPrefs(SpliceTargetPrompt, 1) { Cancelable = true },
                c => c is CostCardModelBase cost && SpliceController.CanSplice(cost, effect))).ToList();
            if (target.Count == 0)
            {
                continue; // ESC 取消选目标 → 放回池里,继续挑别的
            }

            SpliceController.AttachEffect(target[0], effect);
            staged.Remove(effect);
            anythingChanged = true;
            Log.Info($"PartSmith fishing: spliced {effect.Id} back onto {target[0].Id} " +
                     $"(used {SpliceController.UsedPoints(target[0])}/{((CostCardModelBase)target[0]).PointCapacity})");
        }

        return anythingChanged;
    }
}
```

> 注:Phase B 的池里若出现**同一张效果卡两份**(例如拆了两张都拼了 Strike 的卡),网格里会显示两个同名项,取走一个 `staged.Remove` 按引用删一份,行为自洽(见 §6-5)。

### 4.2 `RestSiteOptionInjectionPatch.cs`(新增)

```csharp
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using PartSmith.PartSmithCode.Characters;
using PartSmith.PartSmithCode.RestSite;

namespace PartSmith.PartSmithCode.Patches;

/// <summary>
/// 把"钓鱼"选项挂进篝火(仅大战士)。
/// 注入点 = RestSiteOption.Generate(Player) public static 工厂,
/// RestSiteSynchronizer 每次进篝火对每个玩家调一次;Postfix 追加即可。
/// 与 RewardInjectionPatch 同款 Harmony 注入模式;PatchAll 会自动拾取本类。
/// </summary>
[HarmonyPatch(typeof(RestSiteOption), nameof(RestSiteOption.Generate))]
internal static class RestSiteOptionInjectionPatch
{
    private static void Postfix(List<RestSiteOption> __result, Player player)
    {
        if (player.Character is not BigWarrior)
        {
            return; // 只对大战士生效,不污染原版角色
        }
        __result.Add(new FishingRestSiteOption(player));
    }
}
```

### 4.3 `SpliceController.cs`(修改——追加两个方法)

追加到类尾部,命名空间/现有 using 已覆盖(`BaseLib.Abstracts` 提供 `CardModifier`,`MegaCrit.Sts2.Core.Entities.Cards` 提供 `CardKeyword`):

```csharp
/// <summary>是否已拼接过(卡上存在拼接修饰器)。</summary>
public static bool IsSpliced(CardModel costCard)
    => Attachments(costCard).Count > 0;

/// <summary>
/// 拆解:把一张完整拼卡卸成"干净费用卡(留在卡组)+ 全部效果卡(按原顺序返回)"。
/// 移除全部 <see cref="EffectAttachmentModifier"/>,并对称回收拼接时转移到宿主的关键词
/// (AttachEffect 里 AddKeyword 幂等,这里 RemoveKeyword 对称幂等)。
/// 调用前确认 costCard 是卡组里的实例(不在战斗中,可 mutable)。
/// </summary>
public static IReadOnlyList<EffectCardModelBase> DetachAllEffects(CardModel costCard)
{
    var effects = AttachedEffects(costCard).ToList();
    foreach (var m in Attachments(costCard).ToList())
    {
        CardModifier.RemoveModifier(costCard, m);
    }
    foreach (var e in effects)
    {
        foreach (CardKeyword kw in e.Keywords)
        {
            costCard.RemoveKeyword(kw);
        }
    }
    return effects;
}
```

### 4.4 `CostCardModelBase.cs`(修改——追加一个只读属性)

追加到类尾部(与 `EffectCardModelBase` 同命名空间,无需 using):

```csharp
/// <summary>
/// 来源效果卡(按拼接顺序);未拼接返回 null。
/// 数据本身已由 <see cref="EffectAttachmentModifier"/>(EffectCardId + JoinIndex)持久化,
/// 这里提供只读便捷视图,满足"拼卡需记录来源效果卡(无则 null)"的需求。
/// </summary>
public IReadOnlyList<EffectCardModelBase>? SourceEffectCards
{
    get
    {
        var effects = SpliceController.AttachedEffects(this).ToList();
        return effects.Count == 0 ? null : effects;
    }
}
```

### 4.5 本地化

**`src/PartSmith/PartSmith/localization/eng/rest_site_ui.json`(新增)**

```json
{
  "OPTION_PARTSMITH_FISH.name": "Fish",
  "OPTION_PARTSMITH_FISH.description": "Take apart a spliced card into its cost card and effect cards, then freely recombine them."
}
```

**`src/PartSmith/PartSmith/localization/eng/card_selection.json`(追加键)**

```json
{
  "PARTSMITH_SPLICE_TARGET_PROMPT": "选择要拼接的费用卡",
  "PARTSMITH_SPLICE_REWARD_DESC": "选择一张效果卡,拼到卡组里的费用卡上",
  "PARTSMITH_SPLICE_INSUFFICIENT_POINTS": "点数不足:卡组里没有能拼上这张效果卡的费用卡,请重新选择。",
  "PARTSMITH_FISH_DISASSEMBLE_PROMPT": "选择要拆解的拼卡(可连续拆,ESC 进入重组)",
  "PARTSMITH_FISH_SPLICE_EFFECT_PROMPT": "从奖励池里选择一张效果卡拼回",
  "PARTSMITH_FISH_SPLICE_TARGET_PROMPT": "选择要拼上去的费用卡",
  "PARTSMITH_FISH_CAUGHT": "钓起了 {count} 张效果卡!",
  "PARTSMITH_FISH_NO_SPLICED_CARDS": "卡组里没有可拆解的拼卡。"
}
```

> 现有文件只有 `card_selection.json` 一张表(eng 版是全中文文案)。注意:
> - `rest_site_ui` 是**新增的一张表**。mod 的本地化表是按文件名加载、与原版同名表**合并**的(现有 `cards.json` 与原版 cards 表共存即可证明),所以新增键安全。
> - 若首次实测发现原版篝火选项名(治疗/锻造)被顶掉,说明该环境是"替换"语义,则改用 §6-1 的兜底方案。

**`zhs/`(可选)**:新增 `zhs/rest_site_ui.json` 与 `zhs/card_selection.json` 对应中文;缺省时游戏回退 eng,可不做。

### 4.6 图标(可选)

放一张简单"鱼竿"图标(建议 128×128 PNG)到 `src/PartSmith/PartSmith/images/rest_site/option_partsmith_fish.png`,并把 §4.1 的 `CustomIconPath` 改成对应的 `res://PartSmith/images/rest_site/option_partsmith_fish.png`。不提供就保持 `null`(首次进篝火实测按钮是否正常,无图标不崩即可)。

---

## 5. 交互流程与行为预期

| 场景 | 预期 |
|------|------|
| 大战士进篝火 | 多出一个"Fish(钓鱼)"选项(Heal/Smith 之后) |
| 原版角色(铁甲战士)进篝火 | 不出现钓鱼选项 |
| 点钓鱼后直接 ESC / 卡组无拼卡 | `OnSelect` 返回 false → 选项不消耗,按钮重新可用,可再选治疗/锻造 |
| 拆解一张拼了 [Strike] 的 EmptyShell | 卡组里该卡变干净费用卡(名字回到 Empty Shell);效果卡进池,弹"钓起了 1 张"toast |
| 池里有卡,选效果卡时 ESC | 结束本次钓鱼,池里剩余效果卡**丢弃**;本次篝火已消耗 |
| 重组:池选 Strike → 卡组选 EmptyShell | 拼回,顺序 = 拼接顺序;`partsplice list` 可见 |
| 重组:选目标时 ESC | 效果卡放回池里,继续挑别的(不结束) |
| 拆一张拼卡、拼到**另一张**费用卡上 | 允许(重组目标 = 任意能拼上的费用卡),实现跨卡重组 |
| 拆解后费用卡若已升级 | 升级层数保留在费用卡上;效果增量按宿主 IsUpgraded 实时计算,行为不变 |

---

## 6. 边界情况与注意事项(gotchas)

1. **选项标题走 `rest_site_ui` 表**:`RestSiteOption.Title` 是**非 virtual** 的,硬编码 `new LocString("rest_site_ui", "OPTION_" + OptionId + ".name")`,必须用 `OPTION_PARTSMITH_FISH.name` 这个键。`Description` 是 virtual,可按需 override 到别的表。
   - 兜底:若实测新增 `rest_site_ui.json` 会顶掉原版键,则改用 Harmony patch `RestSiteOption.get_Title` 的 Getter(Prefix 里对本类实例 `ref __result = new LocString("card_selection", "PARTSMITH_FISH_TITLE"); return false;`)。
2. **图标缺失**:`CustomIconPath` 默认 null 时,游戏会尝试加载 `ui/rest_site/option_partsmith_fish.png`(不存在),按钮图标可能为空。首次实测确认不崩溃;不行就补 PNG 或用 CustomIconPath 指到一张已有资源。
3. **关键词回收**:拆解只移除"拼接时转移的关键词"(`effect.Keywords`),与 `AttachEffect` 的转移严格对称。费用卡(EmptyShell 等)自身无关键词,不会误删。若未来有费用卡自带关键词且与效果卡关键词重叠,需再斟酌(当前不影响)。
4. **多人未实现**:`OnSelect` 直接走本地 UI(与 `SpliceReward` 相同的已知局限)。远端玩家的 `Generate` 也会被 Postfix 加选项,但 `OnSelect` 只处理本地交互。
5. **同一效果卡两份**:池里可能出现同一 canonical 实例两份;网格显示为两个同名项;`staged.Remove` 按引用删一份,逻辑自洽。若想更优雅可改为"按 (id, count) 计次"(未来优化,当前不做)。
6. **暂存池是会话内局部变量**:不跨存档/不跨场景;关游戏或切篝火即丢。"奖励长期保留"需要自建 `Reward` 子类 + `ToSerializable`(参考 `Reward.FromSerializable` 的 `SpecialCard` 分支),列为未来工作。
7. **`FromSimpleGrid` 需要 `PlayerChoiceContext`**:单机用 `new BlockingPlayerChoiceContext()`(no-op)。多人需换成真实同步上下文(未实现)。
8. **拆解是"整体拆"**:一次卸掉全部效果。想"只拆单个效果 / 局部调整"是未来扩展(需给 `DetachAllEffects` 加效果过滤 + 相应 UI)。
9. **卡名/卡面/类型动态派生**:`CostCardModelBase` 的 Title/Portrait/Type 都实时读 `AttachedEffects`,拆/拼后下屏重读即自动更新,无需手动刷新。若某屏开着不刷新,调用该卡的 `UpdateVisuals` 或重开屏即可。
10. **不操作卡组 pile**:拆解是原地卸效果,不动 `CardPileCmd`,所以费用卡的卡组位置/升级/附魔全部保留,无需 `RunState.AddCard` 登记新实例。
11. **`RestSiteOption.Generate` 每次进篝火各调一次**:Postfix 每次 new 一个新选项实例,无状态残留问题。
12. **`CardSelectorPrefs(prompt, 1)` 即单选自动确认**(min=max=1、RequireManualConfirmation=false),照 `SpliceReward` 的用法即可。

---

## 7. 构建 / 打包 / 部署 / 验证

> 因为新增了本地化文件(`rest_site_ui.json` 是 pck 里的资源),**必须重新导出 pck** 并连同 dll 一起部署,否则新文本/选项进不了游戏。

1. **构建**(VSCode Ctrl+Shift+B 的 build 任务,或):
   `cd src/PartSmith && dotnet build -p:Sts2Path="D:/Steam/steamapps/common/Slay the Spire 2"`(以项目实际 Sts2PathDiscovery.props 为准)。目标 0 错误。
2. **导出 pck**(本地化在 pck 里):
   `E:/Godot/Godot_v4.5.1-stable_mono_win64/Godot_v4.5.1-stable_mono_win64_console.exe --headless --path src/PartSmith --export-pack "BasicExport" src/PartSmith/dist/PartSmith/PartSmith.pck`
3. **部署**(先关游戏):`bash tools/deploy-to-game.sh`,再把上一步的 `PartSmith.pck` 复制到 `D:/Steam/steamapps/common/Slay the Spire 2/mods/PartSmith/`。
4. **启动**:`D:/Steam/steam.exe -applaunch 2868840`

**游戏内验证**:
1. 开一局大战士;控制台:`partsplice shell`(卡组加 EmptyShell,容量 5)→ `partsplice attach 0 STRIKE_FRAGMENT`(消耗 3)→ `partsplice list` 确认已拼。
2. 走到篝火:应看到"Fish"选项(治疗/锻造之后)。
3. 点 Fish → 选那张拼卡 → 拆解:名字回到 Empty Shell,弹"钓起了 1 张效果卡"toast。
4. 从池里选 Strike → 选 EmptyShell 拼回 → `partsplice list` 确认 [STRIKE_FRAGMENT] 又回来了。
5. 拆解后直接 ESC → 池空、会话结束、效果丢失(预期)。
6. 进钓鱼后立刻 ESC(无任何动作)→ 选项不消耗,还能点治疗/锻造。
7. 打开牌库/卡组界面,确认拼卡名与卡面显示正常(拆/拼后自动重算)。

---

## 8. 反编译参考(实施时核对签名)

| 用途 | 文件 |
|------|------|
| 选项基类/生命周期 | `sts2/MegaCrit.Sts2.Core.Entities.RestSite.RestSiteOption.cs` |
| 选项工厂(注入点) | `sts2/MegaCrit.Sts2.Core.Entities.RestSite.RestSiteOption.cs`(`static Generate`) |
| 执行入口 | `sts2/MegaCrit.Sts2.Core.Multiplayer.Game.RestSiteSynchronizer.cs`(`ChooseLocalOption`→`OnSelect`) |
| 现成选项范例 | `sts2/MegaCrit.Sts2.Core.Entities.RestSite.{Smith,Cook,Heal}RestSiteOption.cs` |
| BaseLib 自定义选项 + 图标 patch | `BaseLib/BaseLib.Abstracts.CustomRestSiteOption.cs`、`BaseLib.Abstracts.CustomRestSiteOptionIconPath.cs` |
| 卡组/网格选择屏 | `sts2/MegaCrit.Sts2.Core.Commands.CardSelectCmd.cs`(`FromDeckGeneric`/`FromSimpleGrid`) |
| 多选屏实现 | `sts2/MegaCrit.Sts2.Core.Nodes.Screens.CardSelection.NSimpleCardSelectScreen.cs` |
| 无 context 单机上下文 | `sts2/MegaCrit.Sts2.Core.GameActions.Multiplayer.BlockingPlayerChoiceContext.cs` |
| modifier 移除/持久化 | `BaseLib/BaseLib.Abstracts.CardModifier.cs`(`RemoveModifier`/`RemoveInternal`/`ModifierSave`) |
| 关键词增删 | `sts2/MegaCrit.Sts2.Core.Models.CardModel.cs`(`AddKeyword`/`RemoveKeyword`) |
| 本地化表合并机制 | `BaseLib/BaseLib.Utils.CustomLocTableManager.cs`、`BaseLib.Patches.Localization.CustomLocTablePatches.*` |
| 现有 mod 参考 | `src/PartSmith/PartSmithCode/Rewards/SpliceReward.cs`(FromDeckGeneric + toast 用法)、`Rewards/SpliceToast.cs`、`Patches/RewardInjectionPatch.cs`(注入模式) |

---

## 附:备选/未来方向(不在本次范围)

- **局部拆解**(只拆某一张效果卡而非整张)需给 `DetachAllEffects` 加过滤 + 选"卸哪张"的 UI。
- **池跨场景持久化**:自建 `Reward` 子类(`ToSerializable` + `Reward.FromSerializable` 的 `SpecialCard` 分支思路),把池挂进存档。
- **鱼塘自定义 UI**(拖拽重排、一次多选):基于 `NSimpleCardSelectScreen` 或自建 Control,当前用现有选择屏组合已满足需求。
