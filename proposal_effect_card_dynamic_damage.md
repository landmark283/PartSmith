# 提案:效果卡描述动态显示实际数值(随所选敌人变化,仿原版)

> 目标:让 mod 的**攻击效果卡**描述不再写死数字,而是像原版一样,在战斗中**选中/悬停某个敌人时,显示打向该敌人时的实际伤害**(含力量加成、目标易伤、虚弱等)。同一机制顺带覆盖**格挡效果卡**(格挡数随敏捷/力量等变化)。
> 状态:**已实施**(84 张效果卡全部接入,含拼卡升级感知;下方 §11 为实施记录与偏离说明)。
> 关联文档:`api-notes.md` §1.4/§2、`partsmith_effect_cards.md` §3(dynamicVars 字段)。

---

## 1. 背景

现有效果卡(`src/PartSmith/PartSmithCode/Cards/EffectCards/`)中,数值全部写死:

- `StrikeFragment.cs`:`DamageCmd.Attack(6m)`(硬编码 6 伤害)
- `cards.json` 描述:`"Deal 6 damage to an enemy."`(硬编码)
- `BlockFragment.cs`:`CreatureCmd.GainBlock(..., 8m, ...)`、描述 `"Splice: Gain 8 Block ..."`(硬编码 8)

原版(如 `StrikeIronclad`)的伤害是**动态数值**:战斗里选中敌人时,卡面描述显示"算上力量/易伤/虚弱后实际打出的伤害",且数字会按目标是否有易伤而变化,被加成时标绿、被削弱时标红。

---

## 2. 原版机制(已反编译验证)

原版卡用三件套实现动态显示:

1. **`CanonicalVars` 定义 `DamageVar`**(伤害数据源):
   ```csharp
   // StrikeIronclad.cs:19
   protected override IEnumerable<DynamicVar> CanonicalVars
       => new[] { new DamageVar(6m, ValueProp.Move) };
   ```
   `OnPlay` 里用 `DynamicVars.Damage.BaseValue` 出伤害(不再写死):
   ```csharp
   await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(this, cardPlay)...
   ```

2. **描述用 `{Damage:diff()}` 占位符**(不是写死数字):
   - `LocString` 文档示例即 `"Deal {Damage:diff()} damage."`
   - 渲染时 `:diff()` 走 `HighlightDifferencesFormatter` → `DynamicVar.ToHighlightedString(false)` → 输出 **`(int)PreviewValue`**,并自动加 `[green]`/`[red]` 颜色标记(预览值>基础值=绿,<基础值=红)。
   - ⚠ 不能写 `{Damage}`(不带 `:diff()`)——那只会渲染**基础值**(`DynamicVar` 实现 IConvertible 返回 `BaseValue`),不带动态预览。

3. **`UpdateDynamicVarPreview` 按当前目标刷新 `PreviewValue`**:
   - UI 层:`NCard.SetPreviewTarget(敌人)`(悬停/选中) → `UpdateVisuals` → `Model.UpdateDynamicVarPreview(previewMode, target, Model.DynamicVars)`。
   - 对 `DamageVar`,`UpdateCardPreview(card, previewMode, target, runGlobalHooks)` 里调
     `Hook.ModifyDamage(..., target, ..., Props, card, ...)`,把力量、目标易伤、虚弱、遗物等全算进 `PreviewValue`(源码 `DamageVar.cs:28-46`)。`target==null` 时不显示易伤加成。

---

## 3. 本 mod 的难点(与方案的由来)

拼接卡描述是 `EffectAttachmentModifier.ModifyDescription` 调 `effect.GetEffectDescription(target)`
(= 效果卡自己的 `GetDescriptionForPile(PileType.None, target)`,见 `EffectCardModelBase.cs:24`)。`target` 能传进去,所以 `{Damage:diff()}` 机制可复用。

**但有个坑**:`NCard.UpdateVisuals` 只对**宿主卡**(拼卡)的 DynamicVars 做 `ClearPreview + UpdateDynamicVarPreview`;效果卡是 ModelDb 里的 canonical 实例,**它的 `PreviewValue` 不会被游戏刷新** → 直接加 `{Damage:diff()}` 只会恒显示基础值,不会随所选敌人变。

**解法**:在 `ModifyDescription` 拼描述前,用**宿主卡上下文**手动刷新效果卡各 DynamicVar 的预览值。宿主卡有 `Owner`/`CombatState`/`Pile`,`DamageVar.UpdateCardPreview(hostCard, ...)` 内部就是用这些算实际伤害。

> 为什么不直接调 `hostCard.UpdateDynamicVarPreview(...)`:该方法被 BaseLib `UpdateModifierPreview` transpiler 补丁改写(见 `BaseLib/BaseLib.Abstracts.UpdateModifierPreview.cs`),调用它会**再次触发各 modifier 的 `UpdateDynamicVarPreview`**,若 modifier 再回调会造成递归/重复刷新。所以逐 var 直调 `UpdateCardPreview` 最干净。

---

## 4. 改动清单(共 4 个文件)

| # | 文件 | 改动 |
|---|---|---|
| 1 | `src/PartSmith/PartSmithCode/Cards/EffectCards/StrikeFragment.cs` | 加 `CanonicalVars` 定义 `DamageVar`;`ExecuteEffect` 用 `DynamicVars.Damage.BaseValue` 替代写死 `6m` |
| 2 | `src/PartSmith/PartSmith/localization/eng/cards.json` | `STRIKE_FRAGMENT.description` 改为 `{Damage:diff()}` 占位符 |
| 3 | `src/PartSmith/PartSmithCode/Cards/Base/EffectCardModelBase.cs` | 新增 `RefreshPreviewForHost(CardModel hostCard, Creature? target)` 公共方法 |
| 4 | `src/PartSmith/PartSmithCode/Cards/Base/EffectAttachmentModifier.cs` | `ModifyDescription` 里、取效果描述前先调 `effect.RefreshPreviewForHost(Owner, target)` |

---

## 5. 逐文件实现

### 5.1 `StrikeFragment.cs`(完整改后)

```csharp
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡示例:点数消耗 3,对目标敌人造成 6 点伤害。伤害走 DynamicVar,描述随目标显示实际数值。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class StrikeFragment : EffectCardModelBase
{
    public StrikeFragment() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 3;

    protected override IEnumerable<DynamicVar> CanonicalVars
        => new[] { new DamageVar(6m, ValueProp.Move) };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        // 宿主卡 = cardPlay.Card(拼卡),攻击来源、攻击动画、力量加成都以它为准。
        // 数值与描述一致:BaseValue(基础值),力量等由 DamageCmd.FromCard 在打出时结算。
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .FromCard(cardPlay.Card, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }
}
```

要点:
- 新增 `using MegaCrit.Sts2.Core.Localization.DynamicVars;`(DynamicVar/DamageVar)与 `using MegaCrit.Sts2.Core.ValueProps;`(ValueProp.Move)。
- `ValueProp.Move` 与 `StrikeIronclad` 一致(`DamageProps.card == ValueProp.Move`),是"正常卡牌攻击伤害"。

### 5.2 `cards.json`

只改这一行:
```json
"PARTSMITH-STRIKE_FRAGMENT.description": "Deal {Damage:diff()} damage to an enemy.",
```

### 5.3 `EffectCardModelBase.cs`(新增方法)

在类内新增(需补 `using MegaCrit.Sts2.Core.Models;` 以引用 `CardModel`;`CardPreviewMode`/`PileType`/`CardUpgradePreviewType` 在已引用的 `MegaCrit.Sts2.Core.Entities.Cards`):

```csharp
/// <summary>
/// 用宿主拼卡的上下文(力量、目标易伤/虚弱等)刷新本效果卡 DynamicVars 的预览值,
/// 让拼接卡描述里 {Damage}/{Block} 等占位符显示"打向该目标时"的实际数值。
/// 仅在展示阶段调用(只改 PreviewValue,不改任何游戏状态)。
/// </summary>
/// <param name="hostCard">拼接后的宿主卡(提供 Owner / CombatState / Pile / 升级预览等上下文)。</param>
/// <param name="target">当前选中的目标敌人;未选中时为 null。</param>
public void RefreshPreviewForHost(CardModel hostCard, Creature? target)
{
    // runGlobalHooks 判定与 CardModel.UpdateDynamicVarPreview 完全一致:
    // 只在战斗中的手牌/打出区(或战斗内升级预览)才跑全局伤害/格挡 Hook(力量/易伤/虚弱)。
    // 卡组/地图等非战斗场景跑全局 Hook 可能拿不到 CombatState,行为不安全,回退为基础值。
    bool runGlobalHooks = hostCard.CombatState != null
        && (hostCard.Pile?.Type is PileType.Hand or PileType.Play
            || hostCard.UpgradePreviewType == CardUpgradePreviewType.Combat);
    foreach (var v in DynamicVars.Values)
    {
        v.UpdateCardPreview(hostCard, CardPreviewMode.Normal, target, runGlobalHooks);
    }
}
```

### 5.4 `EffectAttachmentModifier.cs`(`ModifyDescription`)

在取效果描述前插入刷新调用(改动仅 2 行):

```csharp
public override void ModifyDescription(Creature? target, ref string description)
{
    var effect = ResolveEffectCard();
    if (effect == null)
    {
        return;
    }
    // 用宿主卡上下文刷新效果卡 DynamicVars 的预览值,
    // 描述里的 {Damage:diff()} 等占位符才能显示"打向该目标时"的实际数值。
    if (Owner != null)
    {
        effect.RefreshPreviewForHost(Owner, target);
    }
    // 费用卡自身描述可能为空(点数已显示在右上角),避免拼出来的描述以空行开头。
    string effectDesc = effect.GetEffectDescription(target);
    description = string.IsNullOrEmpty(description)
        ? effectDesc
        : description + "\n" + effectDesc;
}
```

`Owner` 是 `CardModifier.Owner`(挂载时由 `ApplyInternal` 置为宿主卡),public 可读。

---

## 6. 可选的同类改动(格挡效果卡)

同一机制对格挡卡成立,参照原版 `DefendIronclad`:

- `BlockFragment.cs`:加 `CanonicalVars => new[] { new BlockVar(8m, ValueProp.Move) }`;`ExecuteEffect` 改用 `CreatureCmd.GainBlock(cardPlay.Player.Creature, base.DynamicVars.Block, cardPlay)`(有个取 `DynamicVar` 的重载,`DefendIronclad.cs:26` 就是这么用的);描述改 `"Splice: Gain {Block:diff()} Block when the spliced card is played."`。
- `GuardFragment.cs`:同上,数值 2。
- 注意 `BlockVar.UpdateCardPreview` 走 `Hook.ModifyBlock`(敏捷等),与伤害同理。
- `PowerFragment.cs`(力量)暂不改:PowerVar 的显示语义不同(显示的是"效果量"而非"目标身上层数"),若要做需单独调研,不在本提案范围。

---

## 7. 行为预期(改完应看到)

| 场景 | 描述显示 |
|---|---|
| 战斗中,未选敌人 / 目标无易伤 | `Deal 6 damage to an enemy.`(基础值) |
| 战斗中,悬停/选中一个**有易伤**的敌人 | `Deal [green]9[/green] damage ...`(6×1.5,标绿) |
| 战斗中,玩家**力量>0** | 基础值+力量,标绿 |
| 战斗中,玩家**虚弱** | 伤害减半,标红 |
| 地图/卡组界面(非战斗) | 基础值 `6`(与 `DefendIronclad` 在非战斗时一致) |
| 效果卡单独显示(奖励选择屏) | 基础值 `6`(canonical 实例无 owner,`NCard.UpdateVisuals` 会 `ClearPreview`,天然显示基础值) |

---

## 8. 构建 / 部署 / 验证

环境与命令(已核,`src/PartSmith/PartSmith.csproj` + `.vscode/tasks.json` + `tools/deploy-to-game.sh`):

1. **构建**:`.tools/dotnet/dotnet.exe build src/PartSmith/PartSmith.csproj -c Debug`
   - 成功标准:**0 错误 0 警告**(ModAnalyzers 有 STS001/003/004 规则)。
   - 产物落 `src/PartSmith/dist/PartSmith/`(dll/json/pdb)。
2. **重新打包 pck(必做,因为改了 cards.json)**:
   本地化通过 `res://PartSmith/localization/` 从 **pck** 加载,不重打包则描述还是旧文本。
   ```bash
   E:/Godot/Godot_v4.5.1-stable_mono_win64/Godot_v4.5.1-stable_mono_win64_console.exe \
     --headless --path src/PartSmith --export-pack "BasicExport" \
     src/PartSmith/dist/PartSmith/PartSmith.pck
   ```
   (或 `dotnet publish` 走 csproj 的 `GodotPublish` target;确认 `Directory.Build.props` 里 `GodotPath` 已配。)
3. **部署**:先确保**游戏已关闭**(dll 被占用会报 Device or resource busy),再
   ```bash
   bash tools/deploy-to-game.sh          # 拷贝 dll/json/pdb 到游戏 mods
   cp src/PartSmith/dist/PartSmith/PartSmith.pck "D:/Steam/steamapps/common/Slay the Spire 2/mods/PartSmith/"
   ```
   确认游戏 mods 目录下 `PartSmith.dll` + `PartSmith.pck` + `has_pck: true`(manifest 已是)。
4. **启动游戏**:`D:/Steam/steam.exe -applaunch 2868840`(直接跑 exe 有 DRM 检查,不可靠;steam:// 协议不可靠)。
5. **游戏内验证**:
   - 控制台(`` ` `` 或 Shift+8):`card PARTSMITH-EMPTY_SHELL deck` → `card PARTSMITH-STRIKE_FRAGMENT deck`(下一场战斗生效)→ 或 `partsplice` 系列命令直接拼。
   - 战斗中把拼卡拿在手上,**依次悬停/选中不同敌人**,看描述数字是否随目标易伤、随力量变化,并出现绿/红高亮。
   - 打出一张拼卡,确认**实际伤害与描述一致**(走 `BaseValue`,结算与预览同源)。

---

## 9. 注意事项(gotchas)

1. **占位符必须 `{Damage:diff()}`**,不能 `{Damage}`:不带 `:diff()` 渲染的是基础值,动态效果不生效(也丢失绿/红高亮)。
2. **不要**在 `ModifyDescription` 里调 `hostCard.UpdateDynamicVarPreview(...)`:会被 BaseLib transpiler 再次派发到 modifier 预览,可能递归/重复刷新。用 `RefreshPreviewForHost` 里逐 var 的 `UpdateCardPreview` 直调。
3. **canonical 实例共享**:`ResolveEffectCard()` 返回的 canonical 效果卡被同类型所有拼卡共享,其 `PreviewValue` 是瞬态(每次 `ModifyDescription` 都重算),无跨卡污染。Godot 单线程、描述生成串行,安全。
4. **非战斗场景安全**:`RefreshPreviewForHost` 用 `runGlobalHooks` 判定复刻 `CardModel.UpdateDynamicVarPreview`(仅战斗内手牌/打出区才跑全局 Hook),避免 `CombatState==null` 时调 `Hook.ModifyDamage` 出问题。
5. **升级预览(edge case)**:效果卡目前无 `OnUpgrade` 逻辑,升级预览下数字不变,可接受。
6. **以后新增攻击/格挡效果卡**只需三步,无需再动 modifier:
   `CanonicalVars` 定义对应 `DamageVar`/`BlockVar`(数值抄 `partsmith_effect_cards.json` 的 `dynamicVars`)+ `ExecuteEffect` 用 `DynamicVars.X.BaseValue` + 描述写 `{X:diff()}` 占位符。

---

## 10. 参考(已反编译验证)

| 主题 | 位置 |
|---|---|
| `DamageVar` 预览计算(含 `Hook.ModifyDamage`) | `.tools/decompiled/sts2/MegaCrit.Sts2.Core.Localization.DynamicVars.DamageVar.cs` |
| `BlockVar` 预览计算(`Hook.ModifyBlock`) | `.tools/decompiled/sts2/MegaCrit.Sts2.Core.Localization.DynamicVars.BlockVar.cs` |
| `DynamicVar.PreviewValue` / `ToHighlightedString`(绿/红高亮) | `.tools/decompiled/sts2/MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar.cs` |
| 描述组装 + DynamicVars 注册 | `MegaCrit.Sts2.Core.Models.CardModel.cs`(`GetDescriptionForPile:1362`、`UpdateDynamicVarPreview:1452`) |
| UI 刷新入口(SetPreviewTarget→UpdateVisuals) | `MegaCrit.Sts2.Core.Nodes.Cards.NCard.cs`(`SetPreviewTarget:840`、`UpdateVisuals:861`) |
| `{Damage:diff()}` 渲染 | `MegaCrit.Sts2.Core.Localization.Formatters.HighlightDifferencesFormatter.cs`、`LocString.cs` 注释 |
| 原版范本(Strike/Defend) | `MegaCrit.Sts2.Core.Models.Cards.StrikeIronclad.cs` / `DefendIronclad.cs` |
| BaseLib modifier 预览补丁(为什么不能调 hostCard.UpdateDynamicVarPreview) | `.tools/decompiled/BaseLib/BaseLib.Abstracts.UpdateModifierPreview.cs` |

---

## 11. 实施记录(与提案的差异)

已按 §5 方案实施,并做以下调整:

1. **`RefreshPreviewForHost` 加拼卡升级感知**:宿主拼卡 `IsUpgraded` 时,把 `EnchantedValue` 抬到 `基础值+增量`、`PreviewValue` 叠增量(增量来自每卡生成的 `GetUpgradeDelta(string varName)` switch,数据源 = `partsmith_effect_cards.json` 的 `upgrade.vars`)。升级后显示升级值,且绿/红高亮以"升级基线"为比较基准。
2. **修复了生成器的 `PowerVar<T>` 泛型丢失 bug**:旧 `extract_canonical_vars` 的正则 `new \w*Var\(` 匹配不到 `new PowerVar<VulnerablePower>(`,导致 CanonicalVars 首项为泛型时整项被丢弃(Dominate 缺 `VulnerablePower` var、ExecuteEffect 运行时会 KeyNotFound)。已改为 `new \w*Var(?:<[^>]+>)?\(`,并重新生成全部 84 卡。
3. **`GetUpgradeDelta` 键名映射**:`upgrade.vars` 的键是 onUpgradeCode 里的便捷属性名(`DynamicVars.Strength`/`.Vulnerable`),而 var 实际注册名是 `PowerVar<T>.Name`(`StrengthPower`/`VulnerablePower`)。生成时按各卡 CanonicalVars 的实际注册名映射,否则增量永远命中不到。
4. **描述占位符**直接采用原版 `{X:diff()}` 文本(从游戏 pck `localization/eng|zhs/cards.json` 提取,84 卡全命中)。其中 6 卡人工改写:
   - `BodySlam`/`DemonicShield`/`ExpectAFight`/`TearAsunder`:原版有 `{InCombat:(Deals {CalculatedX:diff()} ...)|}` 附注,但 `CalculatedVar.Calculate()` 内部用 `_owner`(效果卡 canonical,`CombatState==null`)→ 乘数恒 0 → 附注只显示基础值(BodySlam/DemonicShield/ExpectAFight 基础值还是 0,会显示 "0")。故删去 `{InCombat}` 附注,保留主体文本。
   - `Blaze`/`Outrage`:原版是多人语境("another player"/"EVERYONE'S"),改为单机适用措辞。
   - `{IfUpgraded:show:A|B}`(Armaments/TrueGrit/Stoke/PrimalForce)按原样保留,但因效果卡是 canonical、`IsUpgraded` 恒 false,恒渲染"未升级"分支(正常拼卡场景正确;升级宿主上不切换文案,已知限制)。
5. **`CalculatedDamage:diff()` 等只显示基础值**:`CalculatedVar.Calculate` 用 `_owner.CombatState`(canonical=null)→ 乘数 0 → 只算 `基础值 + Extra×0`。战斗中也看不到"每个打击牌 +2"的累计总量。这是共享单例架构的固有限制,文案本身(`Deals {ExtraDamage:diff()} additional damage for each Strike.`)仍准确说明机制。
6. **`GetEffectDescription` 增加 `pileType` 重载**,把宿主所在牌堆传给 `GetDescriptionForPile`,使 `{InCombat:...|}` 判定正确(当前 6 张改写卡已不用它,保留机制备后续卡)。
