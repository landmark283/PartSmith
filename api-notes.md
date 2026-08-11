# StS2 Mod 开发 — 已验证 API 笔记

> 用途:本文件记录**已通过反编译源码验证**的 API 结论,供你我日后开发直接查用。
> 验证方式:ILSpy(自包含版)反编译 `sts2.dll` 与 `BaseLib.dll` → 逐条比对源码。
> 反编译产物:`D:\lg\else\mod\.tools\decompiled\{sts2,BaseLib,analyzer}\`(按类型名一个 `.cs` 一个文件)。

---

## 0. 反编译工具链(已建好,勿再造轮子)

| 组件 | 位置 | 说明 |
|---|---|---|
| ILSpy 自包含版(v11.0-rc) | `D:\lg\else\mod\.tools\ILSpy\ilspy\` | 从 github release 手动下载的 zip(51MB),自带 .NET 运行时 |
| 反编译宿主程序 | `D:\lg\else\mod\.tools\decompile\` | 20 行小程序,驱动 ILSpy 的 `CSharpDecompiler` 批量出源码(引擎是 ILSpy 原版) |
| 用法 | `dotnet bin/Debug/net9.0/decompile.dll <in.dll> <outDir> [依赖搜索目录...]` | 依赖搜索目录用游戏 `data_sts2_windows_x86_64`(解析 GodotSharp 等) |

- **更新反编译**:游戏升级后重新跑宿主程序即可(源码变了)。
- 只反编译了 `sts2.dll`(5851 类型)、`BaseLib.dll`(667 类型)、`ModAnalyzers`(18 类型)。

### 0.3 从 SlayTheSpire2.pck 提取 Godot 场景(.tscn)

- **PCK 是 MegaDot(StS2 专用 Godot fork)自定义格式**:头部 `GDPC`+fmt_ver=3+4.5.1,`file_count` 字段不可信,文件索引在**文件尾**且条目格式不标准 → 用现成脚本解析索引容易失败。
- **可靠办法:内容扫描**。场景是明文文本,直接全文件扫 `[gd_scene`(或目标文件的关键串),找到起点后读到下一个 `[gd_scene` 就是完整场景文本。NCard.tscn 在偏移 ~1951355888(每版游戏不同)。
- 场景里的节点坐标是定位 UI 的唯一权威(如卡面能量图标 `CardContainer` 局部坐标 `(-166..-102,-227..-163)`)。脚本在 `$CLAUDE_JOB_DIR/tmp/{extract_scenes,scan_ncard}.py`,可复用。

---

## 1. 自定义卡牌(已验证)

### 1.1 定义一张卡 — 最小模式

继承 BaseLib 的 `CustomCardModel`(它在 `CardModel` 之上加了自动注册 + 本地化接口):

```csharp
using BaseLib.Utils;                                        // PoolAttribute
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models.CardPools;                  // IroncladCardPool

[Pool(typeof(IroncladCardPool))]                            // 关键:塞进铁甲战士卡池
public class InstantWinCard : CustomCardModel
{
    public InstantWinCard() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None) { }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 打出效果写这里
    }
}
```

### 1.2 构造签名(源码:`decompiled/BaseLib/BaseLib.Abstracts.CustomCardModel.cs:71`)

```csharp
public CustomCardModel(int baseCost, CardType type, CardRarity rarity, TargetType target,
                       bool showInCardLibrary = true, bool autoAdd = true)
```

- `autoAdd:true`(默认)→ 构造时自动调 `CustomContentDictionary.AddModel(GetType())` 完成注册,无需手动注册。

### 1.3 卡池注册机制(源码:`decompiled/BaseLib/BaseLib.Patches.Content.CustomContentDictionary.cs`)

1. `[Pool(typeof(池类型))]`(`PoolAttribute`,在 `BaseLib.Utils`)指定归属池。
2. `AddModel` 读取 `[Pool]` → 校验池类型与模型类型匹配 → `ModHelper.AddModelToPool(poolType, modelType)`。
3. 官方池(游戏内):
   - **`MegaCrit.Sts2.Core.Models.CardPools.IroncladCardPool`**(铁甲战士)✅ 已确认存在
   - `SilentCardPool` / `DefectCardPool` / `NecrobinderCardPool` / `RegentCardPool` / `ColorlessCardPool` / `CurseCardPool` / `StatusCardPool` / `TokenCardPool` …

### 1.4 卡牌基类 `CardModel` 关键成员(源码:`decompiled/sts2/MegaCrit.Sts2.Core.Models.CardModel.cs`)

- 构造:`CardModel(int baseCost, CardType type, CardRarity rarity, TargetType target, bool showInCardLibrary = true)`
- 打出效果:`protected virtual Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)` —— 子类 override
- 升级:`protected virtual void OnUpgrade()`
- 动态数值:`DynamicVars`(Damage/Block 等)、`CanonicalVars` / `CanonicalTags`
- 真实游戏卡参考:`decompiled/sts2/MegaCrit.Sts2.Core.Models.Cards.Clash.cs`(0费、`DamageCmd.Attack(...).FromCard(this, cardPlay).Targeting(target)`)

#### 卡面能量费用图标(已验证 2026-08-09)

- `CardModel.EnergyIcon`(`CardModel.cs:255`)= `ResourceLoader.Load<Texture2D>(EnergyIconPath)`,其中 `EnergyIconPath`(`:253`)= `VisualCardPool.EnergyIconPath`(`VisualCardPool => Pool`,`:325` 可 override 到别的池)。
- `CardPoolModel.EnergyIconPath`(`CardPoolModel.cs:39`)= `EnergyIconHelper.GetPath(EnergyColorName)`;`GetPath(prefix)`(`EnergyIconHelper.cs:42`)= `atlases/ui_atlas.sprites/card/energy_{prefix.ToLowerInvariant()}.tres`(atlas 子图)。原版池如 `IroncladCardPool.EnergyColorName => "ironclad"` → `res://images/atlases/ui_atlas.sprites/card/energy_ironclad.tres`。
- **自定义池** `CustomCardPoolModel.EnergyColorName` = `<category>∴<entry>`(BaseLib 通用实现,`CustomCardPoolModel.cs:23`;自定义卡池分类通常即 card_pool),BaseLib `CustomEnergyIconPatches.IconPatch`(文件 `BaseLib/BaseLib.Patches.UI.CustomEnergyIconPatches.IconPatch.cs`,prefix 在 `:14-17`)拦截 `EnergyIconHelper.GetPath(string)`:prefix 含 `∴` 且 `ModelDb.GetById(池)` 是 `ICustomEnergyIconPool` 且 **`BigEnergyIconPath != null`** → 直接返回该路径;否则回落原逻辑 → 加载 pck 里**不存在**的 `energy_{card_pool∴...}.tres` → 图标 null(日志 `Missing sprite 'card/energy_card_pool∴...' in ui_atlas`)。
- **修复(PartSmith 先例)**:两个卡池 override `BigEnergyIconPath` = `res://images/atlases/ui_atlas.sprites/card/energy_ironclad.tres`(原版图标)+ `TextEnergyIconPath` = `res://images/packed/sprite_fonts/ironclad_energy_icon.png`(描述内嵌 `{EnergyIcon}` 文本图标)+ `EnergyOutlineColor` = 对应原版色(ironclad=`#802020`;`CardPoolModel.cs:33-36` 注释要求描边色必须与图标融合)。`NCard.cs:1184 _energyIcon.Texture = Model.EnergyIcon`。效果卡/费用卡奖励屏、卡组、手牌都走这条链。

### 1.5 枚举值

- `TargetType`:`None / Self / AnyEnemy / AllEnemies / RandomEnemy / AnyPlayer / AnyAlly / AllAllies / TargetedNoCreature / Osty`(`TargetType.None` = 无需选择目标,适合"打出即胜"这种全局效果)
- `CardRarity`:`None / Basic / Common / Uncommon / Rare / Ancient / Event / Token / Status / Curse / Quest`
- `CardType`:`None / Attack / Skill / Power / Status / Curse / Quest`(已验证 CardType.Skill 存在)

---

## 2. 本地化(已验证)

### 2.1 三条硬性规则(来自 `Alchyr.Sts2.ModAnalyzers`,详见 `decompiled/analyzer/ModAnalyzers.LocalizationAnalyzer.cs`)

| 规则 | 级别 | 含义 |
|---|---|---|
| **STS001** | Error | 卡片必须在 `cards.json` 里有 `{id}.title` 和 `{id}.description` |
| STS003 | Warning | 模型应继承 `BaseLib.Abstracts.Custom*Model` |
| STS004 | Warning | 模型必须用 `[Pool]` 加进池子 |

### 2.2 id 的生成算法(帮助方法 `decompiled/analyzer/ModAnalyzers.Extensions.cs:151-171`,组装在 `ModAnalyzers.LocalizationAnalyzer.cs:198` + 运行时确认)

```csharp
id = GetPrefix() + Name.Slugify()
GetPrefix()  = 根命名空间大写 + '-'      // 命名空间 PartSmith.*  → "PARTSMITH-"
Slugify()    = CamelCase → SCREAMING_SNAKE  // InstantWinCard → INSTANT_WIN_CARD
```
> 组装实际发生在 `ModAnalyzers.LocalizationAnalyzer.cs:198`(`id = prefix + name.Slugify()`);`Extensions.cs:151-171` 只定义 Slugify/GetPrefix/GetRootNamespace 帮助方法。

**例**:`InstantWinCard`(命名空间 `PartSmith.PartSmithCode.Cards`)→ id = `PARTSMITH-INSTANT_WIN_CARD`

**运行时机制已验证**(比分析器更重要的是游戏侧):
- 游戏 `ModelDb.GetEntry(Type)` = `StringHelper.Slugify(type.Name)`(无前缀,`decompiled/sts2/MegaCrit.Sts2.Core.Models.ModelDb.cs:535`)。
- BaseLib `PrefixIdPatch` 给 `ICustomModel` 的 entry 前加 `GetPrefix()`(首段命名空间大写+`-`,`decompiled/BaseLib/BaseLib.Patches.Content.PrefixIdPatch.cs` + `BaseLib.Extensions.TypePrefix.cs`)。
- 分析器的 id 算法 = 运行时算法,**两者一致**(这也解释了为什么 STS001 能验证本地化 key 正确)。

### 2.5 控制台加卡/抽卡命令(已验证,无需打补丁)

| 命令 | 作用 |
|---|---|
| `card PARTSMITH-INSTANT_WIN_CARD hand` | 战斗中加进**手牌**(最快测试) |
| `card PARTSMITH-INSTANT_WIN_CARD deck` | 加进**永久牌组**(房间间保留,进战斗克隆进抽牌堆;地图界面即可用) |
| `card <id> draw/discard/exhaust` | 加进对应堆 |
| `draw <数量>` | 抽卡 |
| `remove_card <id>`(默认 Hand;显式传 `Deck` 才作用于牌组) | 从手牌/牌组移除 |

- 控制台开关:`` ` ``(backtick)或 Shift+8;Escape 关闭。
- `card` 命令解析 id 时 `ToUpperInvariant` 匹配 `Id.Entry`,并按 pile 类型自动选作用域(战斗堆用 CombatManager,`deck` 用 RunManager)——所以 `deck` 加卡要在一局进行中(地图界面也行)。
- 需要 `ModelDb.AllCards` 里有这张卡(`[Pool]` 注册后即有)。

### 2.3 `cards.json` 格式

放 `PartSmith/localization/eng/cards.json`:

```json
{
  "PARTSMITH-INSTANT_WIN_CARD.title": "Instant Win",
  "PARTSMITH-INSTANT_WIN_CARD.description": "Win the battle."
}
```

运行时 BaseLib 会把 `{entry}.title` / `{entry}.description` 注册进 "cards" 本地化表(源码:`decompiled/BaseLib/BaseLib.Patches.Localization.ModelLocPatch.cs`)。

> 也可在代码里 override `Localization` 属性返回 `new CardLoc(...)` 走 `ILocalizationProvider`,二选一即可;用 JSON 更符合模板惯例。

### 2.4 卡图

- 模板基类 `PartSmithCard` 用 `Id.Entry.RemovePrefix().ToLowerInvariant()` 拼图路径(如 `instant_win_card.png`)。
- **找不到图会自动回退到占位图 `card.png`**(源码:`PartSmithCode/Extensions/StringExtensions.cs` 的 `CardImagePath`/`BigCardImagePath`),所以原型阶段**不用**做卡图。

#### 效果卡取卡图逻辑(已验证,2026-08-10)

效果卡不做图,1:1 复用原版卡原画。每张效果卡 override 一行:

```csharp
// 例:UppercutFragment(所有 effect card 都是这个模式)
protected override CardModel? PortraitSourceCard => ModelDb.Card<Uppercut>();
```

`EffectCardModelBase`(源码 `PartSmithCode/Cards/Base/EffectCardModelBase.cs`)从 `PortraitSourceCard` 派生全部卡图路径:

| 成员 | 值 | 用途 |
|---|---|---|
| `PortraitPath` | 原版卡 atlas(`atlases/card_atlas.sprites/ironclad/uppercut.tres`) | 拼卡单张卡面 |
| `CustomPortrait` | `PortraitSourceCard?.Portrait`(原版 atlas 纹理,非 null) | 奖励选择屏/预览显示 |
| `BetaPortraitPath` | 同 `PortraitPath` | beta 卡面 |
| `PortraitPngPath`/`SplicePngPath` | 原版独立 PNG(`packed/card_portraits/ironclad/uppercut.png`) | 拼图切片源(`CardArtSplicer` 用 `Texture2D.GetImage()` 切片合成) |
| `SpliceBetaPngPath` | 原版 beta PNG | 少数卡(Blaze/Outrage)无普通原画时回退 |

**拼图(≥2 张效果)** 走 `CostCardModelBase` override `CustomPortrait` + `CardArtSplicer` 运行时 `Image.BlitRect` 切片合成(见 card_art_splicing_plan.md),切片源 = `SplicePngPath`。

**`ModelDb.Card<T>()` 按 C# 类型名解析原版卡模型**(`typeof(T).Name`)。⚠️ **坑:基础游戏英文卡名与中文名并非直觉对应**,用错类型会取到错误的卡图:

| 你想要的 | 基础游戏 C# 类(必须用这个) | 别写成 |
|---|---|---|
| 践踏(3费攻击) | `ModelDb.Card<Stomp>()` | ❌ `Card<Stampede>()` → 那是"惊逃" |
| 惊逃(2费能力) | `ModelDb.Card<Stampede>()` | — |

查中文名→英文类名,可用游戏内 `card <类名>` 加卡后读 `/state` 的 title,或反编译 `decompiled/sts2/MegaCrit.Sts2.Core.Models.Cards.*.cs` 的构造签名(cost/type)。

---

## 3. "直接获胜"的实现(已验证,最重要)

### 3.1 游戏胜利判定

- 战斗胜利 = 场上没有存活的**主要敌人**(`IsPrimaryEnemy`)+ 无东西阻止结束(`Hook.ShouldStopCombatFromEnding`)。
- 判定入口:`CombatManager.CheckWinCondition()`(public)→ `EndCombatInternal` 走完整胜利流程(奖励、`AfterCombatVictory` hook、存档等)。
- **每个动作执行完都会自动调 `CheckWinCondition`**(源码:`decompiled/sts2/MegaCrit.Sts2.Core.GameActions.ActionExecutor.cs:170`)——所以打完卡杀光敌人,立刻判胜。

### 3.2 官方"win"控制台命令的源码(照抄这个模式)

源码:`decompiled/sts2/MegaCrit.Sts2.Core.DevConsole.ConsoleCommands.WinConsoleCmd.cs`(官方用私有 `KillEnemies` 方法包裹,`:40-48`,语义如下):

```csharp
private async Task KillEnemies(List<Creature> creatures)
{
    foreach (Creature c in creatures)
    {
        c.RemoveAllPowersInternalExcept();   // 清空防死亡效果(如护身符);public 方法
        await CreatureCmd.Kill(c);           // 单参数,无 force:true
    }
    await CombatManager.Instance.CheckWinCondition();
}
```

### 3.3 卡牌 OnPlay 里的等价实现

```csharp
protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
{
    var enemies = CombatManager.Instance.DebugOnlyGetState()?.Enemies;
    if (enemies == null) return;
    foreach (var e in enemies.ToList())
        await CreatureCmd.Kill(e);       // CreatureCmd.Kill(Creature, bool force=false) public
    await CombatManager.Instance.CheckWinCondition();
}
```

> `RemoveAllPowersInternalExcept` 实际是 **public**(`Creature.cs:663`,签名 `public IEnumerable<PowerModel> RemoveAllPowersInternalExcept(IEnumerable<PowerModel>? except = null)`),mod 可直接调。原型阶段也可省略——`Kill` 本身会走死亡处理。若遇到"怪物有防死亡效果"再补上。

### 3.4 相关公开 API 确认

| 类型 | 成员 | 可见性 |
|---|---|---|
| `CombatManager`(public) | `Instance`, `DebugOnlyGetState()`(public,标注 tests-only 但官方 win 命令也在用), `CheckWinCondition()`, `LoseCombat()`(败北用), `CurrentCombatId` | 均可用 |
| `CombatState`(public) | `Enemies` → `IReadOnlyList<Creature>`, `Players` | 可用 |
| `CreatureCmd`(public static) | `Kill(Creature, bool force=false)` / `Kill(IReadOnlyCollection<Creature>, ...)` | 可用 |

---

## 4. 常用命令摘要(游戏自带,参考)

| 命令 | 源码 | 作用 |
|---|---|---|
| `win` | `WinConsoleCmd.cs` | 杀所有敌人 + 判胜 |
| `kill all` | `KillConsoleCmd.cs` | 杀指定/所有敌人 |

---

## 5. 其他备查

- **Power 类**也有 `CustomPowerModel`(`BaseLib.Abstracts`),模板已带 `PartSmithPower.cs`。
- 模板自带内容:`PartSmithCard.cs`(卡基类)、`PartSmithRelic.cs`、`PartSmithPower.cs`、`cards.json`/`powers.json`/`relics.json` 等本地化文件、占位图。
- 想要更多官方 API 细节:直接在 `decompiled/sts2/` 里 grep 关键字(比在线文档全)。

---

## 6. 卡片拼接机制(PartSmith 核心,已反编译验证 2026-08-08)

> 机制:打牌规则不变(能量 3/回合、抽 5);点数只在拼接时起作用。拼接载体 = `CardModifier` 实例挂在费用卡实例上。

### 6.1 拼接载体 = CardModifier(实例级,BaseLib)

- 每效果一个 `EffectAttachmentModifier : CardModifier`,挂在费用卡实例上。
- **执行**:BaseLib `AfterCardPlayedPatch`(patch `Hook.AfterCardPlayed`)在每张卡打出后**按列表序**调每个 modifier 的 `OnPlay(choiceContext, cardPlay)`。挂载用 `InsertSorted`(按 `Priority` 排序,相等 append)→ 令 `Priority = JoinIndex` 即可保序。
- **持久化**:`CardModifier.RegisterSave()`(需 manifest `affects_gameplay:true`)注册 `"BaseLibCardModifiers"` 到 `ExtendedSaveTypes`。`StoreSaveData(ModifierSave)` / `LoadSaveData(ModifierSave)` 存自定义字段。**Priority 不进存档** → `LoadSaveData` 里必须 `Priority = JoinIndex` 兜底。
- **克隆进战斗**:`_modifiers` SpireField `CopyOnClone`(CardModifier.cs 静态构造)自动带上,顺序保留。
- **取实例禁止 `new`**(撞 `DuplicateModelException`):`(EffectAttachmentModifier)CardModifier.Get<EffectAttachmentModifier>().MutableClone()`。`CardModifier.Get<T>()` = `ModelDbExtensions.CardModifier<T>()`(BaseLib.Extensions)= `ModelDb.Get<T>()`。
- 静态 API:`Modifiers(card)`(ReadOnlyCollection)/`DirectModifiers(card)`(List)/`AddModifier(card, modifier)`(内部 ApplyInternal→InsertSorted)/`RemoveModifier(card, modifier)`。
- `ModifierSave`:字段 `IntProperties`(Dict<string,int>)、`AdditionalProperties`(Dict<string,string>)。`ModelId` 存字符串:`Id.ToString()`=`"Category.Entry"`,`ModelId.Deserialize(str)` 反向解析。
- 效果卡引用:modifier 存 `effectCard.Id.ToString()`;`ModelDb.GetById<EffectCardModelBase>(ModelId.Deserialize(str))` 解析 canonical 实例。

### 6.2 拼卡显示三个 seam(全 virtual,可按实例动态)

| seam | 位置 | 说明 |
|---|---|---|
| 名字 | `CardModel.Title`(`public virtual string`,CardModel.cs:110);NCard 标题走 `Model.Title`(NCard.cs:1311) | 覆盖返回效果名逗号连接;升级后缀逻辑照抄原版(`IsUpgraded`/`MaxUpgradeLevel`/`CurrentUpgradeLevel` 均 public) |
| 卡面 | `CardModel.PortraitPath`/`BetaPortraitPath`(virtual,:143/145) | 官方 MadScience/Wither 已按实例状态动态返回 → 覆盖返回第一张效果卡的图 |
| 目标类型 | `CardModel.TargetType`(`public virtual`,:497) | 拼了需要目标的效果 → 返回效果的目标类型;否则 base |
| 描述 | BaseLib `DescriptionOverrides.CustomizeDescription` 事件(patches `GetDescriptionForPile`) | `CardModifier.ModifyDescription(target, ref desc)` 已挂,按列表序追加;效果文本用 `effectCard.GetDescriptionForPile(PileType.None, target)` |
| 类型 | `CardModel.Type`(`public virtual CardType`,CardModel.cs:289)+ `GainsBlock`(virtual,:671),`CustomCardModel` 均未覆写 | 拼卡由效果派生:有 Power 效果→Power,否则有 Attack 效果→Attack,否则 base(技能)。原因:按类型分类的内容(如 SelfHelpBook 附魔按 攻击/技能/能力 筛卡,`card.Type == type`)需要拼卡类型跟实际效果一致 |

### 6.3 注册与自定义池

- `[Pool]` 放在**每个具体卡**上(`PoolAttribute` 是 `Inherited=true`、`GetCustomAttribute` 默认 `inherit:true`,基类带 `[Pool]` 默认会**继承**;真正要求放具体卡的原因是 `ReflectionHelper.GetSubtypesInMods` 只收集**非抽象叶子**类型)。
- `IsValidPool` 走 `poolType` 基类链找 `CardPoolModel`/`RelicPoolModel`/`PotionPoolModel` → 自定义池继承 `CustomCardPoolModel` 即可通过校验。
- `CustomCardPoolModel : CardPoolModel, ICustomModel, ICustomEnergyIconPool`(BaseLib.Abstracts)。具体池必须实现抽象成员:`Title`、`DeckEntryCardColor`、`IsColorless`(`CardFrameMaterialPath`/`EnergyColorName` 已由 CustomCardPoolModel 实现)。
- 池/卡/modifier 均由 `ReflectionHelper.GetSubtypesInMods<AbstractModel>()` 自动进 ModelDb(ModelDb.cs:81)。

### 6.4 打牌流程(拼卡 = 普通卡)

拼卡就是普通卡:能量检查/支付走原版(`CanPlay`→`SpendResources`→`OnPlayWrapper`)。效果执行顺序 = 费用卡自身 `OnPlay` → 各 modifier `OnPlay`(列表序)。

### 6.5 控制台命令(mod 自动发现)

- `DevConsole.cs:33`:`AbstractConsoleCmdSubtypes.All.Concat(ReflectionHelper.GetSubtypesInMods<AbstractConsoleCmd>())` → 继承 `AbstractConsoleCmd` 即可,无需注册。
- `CmdResult`:同步 `new CmdResult(bool, string?)`;async 操作 `new CmdResult(Task, bool, string?)`。

### 6.6 奖励/拼接 UI(M2 已验证并部署 2026-08-09)

- 注入点:`RewardsSet.GenerateRewardsFor(Player, AbstractRoom)`(**private 实例方法**,RewardsSet.cs:206)Harmony Postfix。⚠ 私有方法不能 `nameof`,用字符串 `[HarmonyPatch(typeof(RewardsSet), "GenerateRewardsFor")]`。Postfix 签名 `(List<Reward> __result, Player player, AbstractRoom room)` 取返回值+参数。此刻奖励**未 Populate**,可安全替换。房间类型判断:`room.RoomType == RoomType.Monster`(精英=Elite/Boss=Boss)。
- 费用卡奖励 = vanilla `CardReward`:`new CardCreationOptions(new[]{ ModelDb.CardPool<自定义池>() }, CardCreationSource.Encounter, CardRarityOddsType.Uniform)` → `new CardReward(options, 3, player)`(构造函数自己会加 `IsCardReward`)。
- **`ModelDb.Get<T>()` 不存在!** 取池用 `ModelDb.CardPool<T>()`,取卡 `ModelDb.Card<T>()`,取遗物 `ModelDb.Relic<T>()`,取模型实例按 id 用 `ModelDb.GetById<T>(ModelId)`。
- **`CardReward.ToSerializable` 硬约束**:`Options.Flags & ~IsCardReward` 必须为 0 → 不能带 `NoUpgradeRoll`/`IsFromCombat` 等任何 flag,也不能有 `CardPoolFilter`。否则存档时抛 NotImplementedException。所以:
  - **不要** `ForRoom(...)`(会带 `IsFromCombat`)→ 手动 `new CardCreationOptions(...)`。
  - **不要** `NoUpgradeRoll` → 靠卡自身不开升级(稀有卡 upgrade odds=0;`RollForUpgrade` 先查 `IsUpgradable`)。
  - ⚠ **不要**用 `CardReward(IEnumerable<CardModel>,...)` 手动卡构造——`Options.CardPools` 为空,`ToSerializable()` 抛异常。
- 效果卡奖励 = `SpliceReward : CardReward` 只重写 `OnSelect`(virtual,`SelectUnsynchronized` 会调它;返回 true=已领取/触发 AfterRewardTaken,false=未领取可跳过):
  - `Populate()` 里 `if (!IsPopulated) base.Populate()`(基类 `_cards` 私有,读公有 `Cards` 属性)。
  - 选效果:`NCardRewardSelectionScreen.ShowScreen(IReadOnlyList<CardCreationResult>, IReadOnlyList<CardRewardAlternative>)` + `await screen.OptionSelected()`(Task<int?>,null=跳过)。用 `cards.Select(c => new CardCreationResult(c)).ToList()`(CardCreationResult 构造公开)重建列表。
  - 选目标:`CardSelectCmd.FromDeckGeneric(Player, new CardSelectorPrefs(LocString, 1), filter)`——filter 里 `c is CostCardModelBase cost && SpliceController.CanSplice(cost, effectCard)`;恰好 1 个候选会自动选中不弹 UI,0 个返回空。
  - 用完 `NOverlayStack.Instance?.Remove(screen)`。
  - 排序:`public override int RewardsSetIndex => 6`(金币=1/药水=2/遗物=3/卡=5),排费用槽后。`Description` 可覆写(用自定义 loc key)。
  - ⚠ 多人同步(2026-08-10 修复,见 §8):OnSelect 由 RewardsSetSynchronizer 在**所有机器**上跑。效果卡选择要先 `ReserveChoiceId(Player)`(全机器),owner `SyncLocalChoice(FromIndex(selected))` / 远端 `WaitForRemoteChoice(...).AsIndexOrNull()`;**不要**写 `if (!LocalContext.IsMe(Player)) return false;` 提前返回——远端少 reserve 一次,choice 计数器永久漂移,每次校验都报"信息不同步"。
  - **`NCardRewardSelectionScreen.OptionSelected()` 被移除屏幕时抛 `TaskCanceledException`**(`_ExitTree` 里 `SetException`),**不会返回 null**;其 `SetResult` 只传卡片 index 或 alternative index(≥cards.Count),没有 null 路径。所以 catch TaskCanceledException = 跳过。
  - **拼接失败不要内部循环**:失败(点数不足)/取消选目标 → 直接 `return false` 退回奖励页面让玩家重击。内部循环(移除失败卡+重开选择屏)会造成 base CardReward 的 `_cards`/历史记账与展示不一致——效果卡没被拿走但选项像少了。
  - **卡面右上角角标**(`NCardPointLabelPatch` 先例):patch `NCard._Ready`(加 Label)+ `NCard.Reload`(private,按名 patch)刷新。坐标=镜像左上角能量图标:卡中心 x=12,能量在 CardContainer 局部 `(-166..-102,-227..-163)` → 右上角 `(126..190,-227..-163)`,随卡缩放。字体 `res://themes/kreon_bold_shared.tres`。卡片场景布局从 `SlayTheSpire2.pck` 提取 `NCard.tscn` 得知(见 §0.3)。
  - **卡组选择要可取消必须 `new CardSelectorPrefs(prompt, 1) { Cancelable = true }`**:`NDeckCardSelectScreen` 的关闭按钮(`%Close`/`CloseSelection`)只在 `_prefs.Cancelable` 时启用,点了 `SetResult(Array.Empty<CardModel>())` → `FromDeckGeneric` 返回空 → 判为"玩家取消"。
  - **临时浮动提示(toast)**:自己 `new Label` 挂 `NRun.Instance.GlobalUi`(NGlobalUi, Control),锚点固定顶边中点(`AnchorLeft=0.5` 等)+ 偏移 800x65 框,`AddThemeFontSizeOverride/Color/ConstantOverride` 设置样式,`CreateTween` 淡入淡出,`await label.ToSignal(fade, Tween.SignalName.Finished)`(注意 `Tween.Finished` 是事件不能 await),`Cmd.Wait(秒, ignoreCombatEnd:true)` 控制停留。Label 的枚举是 `Control.MouseFilterEnum.Ignore`。
- **mod 本地化只 merge 基础表同名文件**:`LocManager.LoadTablesFromPath` 遍历基础游戏 `res://localization/{lang}/*.json`(scope=文件名),对每个基础表文件再 `ModManager.GetModdedLocTables(language, file)` 找 `res://{modId}/localization/{language}/{file}` 并 `MergeWith`。所以 file 必须对应**基础游戏已存在的 scope**(cards.json/characters.json/ancients.json/card_selection.json/card_library.json…),否则新文件不会被枚举。**精确规则**:能用新文件名,只要该 scope 基础表存在——`card_library` 是原版已有 scope,mod 新建 `localization/eng/card_library.json` 可合并(2026-08-09 日志实测 `Found loc table from mod: eng card_library.json. Merging with base loc table`)。完全原创的 scope 才必须塞进同名文件。例:拼接 prompt key `PARTSMITH_SPLICE_TARGET_PROMPT` 放 `card_selection.json`。
- **百科大全(CardLibrary)显示原理(完整版见 §6.7,2026-08-09 落地 + 08-10 数据层验证)**:图鉴显示 = 收卡(`ShouldShowInCardLibrary`)→ 池过滤(单选按钮)→ 可见性(unlocked/seen)。自定义池上图鉴需:`CustomCardPoolModel` + `IsShared=>true` + `SeenByDefault=>true` + `[Pool]` 卡 + 新增池过滤器按钮(`PartSmithCardLibraryFiltersPatch`,postfix `NCardLibrary._Ready` Priority.First)。按钮构造细节 / Harmony 顺序实证 / 自检命令 `parttest library` 均见 §6.7。
- 奖励基类:`Reward.OnSelect()` protected、`SelectUnsynchronized()` public、`Hook.ModifyRewards`/`BeforeCombatRewardOffered` 可干预。BaseLib `CustomReward` + `CustomRewardPatches.RegisterCustomReward` 可自定义奖励类型(范例 `BaseLib.Common.Rewards.*`)。

## 7. 百科大全(CardLibrary)显示原理(已反编译验证 + 游戏内数据层验证 2026-08-10)

> 玩家在主菜单「百科大全」看到的卡牌图鉴,显示与否 = **收卡 → 池过滤 → 可见性** 三段;mod 卡想上图鉴要同时满足这三条。

### 7.1 入口

- 主菜单 → 「百科大全」(`main_menu:compendium`)→ `NCompendiumSubmenu` → 「卡牌图鉴」按钮 → `OpenCardLibrary()` → 把 `NCardLibrary` 子菜单 push 到 `_stack`。
- ⚠ 桥(`/action`)只暴露 `main_menu:compendium`,compendium 子菜单**内部**的标签页按钮不进 `available_actions` → 无法用桥点「卡牌图鉴」标签;调试时需另写命令直接反射调 `OpenCardLibrary` 或读屏幕节点。

### 7.2 收卡(`NCardLibraryGrid._Ready`,NCardLibraryGrid.cs:151)

```csharp
_allCards = ModelDb.AllCards.Where(c => c.ShouldShowInCardLibrary).ToList();
_allCards.Sort(new InitialSorter(ModelDb.AllCardPools.ToList())); // 池顺序→稀有度→id
RefreshVisibility();
```

- **`ShouldShowInCardLibrary` 是唯一入场门槛**:`CardModel` 构造参数(默认 true,CardModel.cs:1087-1093),全游戏**只**图鉴用。个别原版卡显式传 false 排除(Deprecated/MadScience)。自定义卡想上图鉴 → 构造传 true(`PartSmithCard` 有 `showInCardLibrary` 透传,效果/费用基类都传 true)。
- 之后所有筛选都在 `_allCards` 上做(`FilterCards(filter)`),`_allCards` 本身只收一次。

### 7.3 池过滤(`NCardLibrary`,单选 radio)

- `_poolFilters: Dictionary<NCardPoolFilter, Func<CardModel,bool>>` 池按钮→谓词(原版 `c.Pool is IroncladCardPool` 等;Ancients=稀有度、Misc=稀有度≥6)。
- `UpdateFilter()`(NCardLibrary.cs:902)组合:**`(cost Any) && (rarity Any) && (type Any) && (pool Any) && 搜索文本 && multiplayer`**,各类别内部是 `Any`(多选勾选)。⚠ 池类别若一个都没选中,`poolFilter.Any(...)` 为 false → **整屏空白** → 所以任意时刻必有且只有一个池按钮被选中(radio 互斥)。
- **默认选中哪个池** = `OnSubmenuOpened`:`key.IsSelected = _cardPoolFilters[本地角色] == key`。原版映射 Ironclad/Silent/Defect/Necrobinder/Regent → 各自池按钮;自定义角色需 patch 补映射。

### 7.4 可见性(`NCardLibraryGrid.GetCardVisibility`,NCardLibraryGrid.cs:232)

```
不在 _unlockedCards → Locked(锁定剪影)
unlocked 但不在 _seenCards  → NotSeen(未见剪影)
都在 → Visible(完整卡面)
```

- `_unlockedCards` = `ModelDb.AllCardPools.SelectMany(p => p.GetUnlockedCards(unlockState, CardMultiplayerConstraint.None))`;`_seenCards` = `SaveManager.Instance.Progress.DiscoveredCards`;`unlockState` = `SaveManager.Instance.GenerateUnlockStateFromProgress()`。
- **解锁**:`CardPoolModel.GetUnlockedCards`(CardPoolModel.cs:101)= `FilterThroughEpochs(unlockState, AllCards)` → 再按多人约束过滤;基类 `FilterThroughEpochs` 默认**全放行** → 自定义池卡默认全解锁。
- **已见**:池是 `CustomCardPoolModel { SeenByDefault: not false }` → BaseLib `CustomCardPoolMarkAsSeenPatch`(Harmony **prefix** `NCardLibraryGrid.RefreshVisibility`)遍历 `pool.AllCards` 逐个 `SaveManager.Instance.MarkCardAsSeen`。所以想让自定义池卡上图鉴**直接完整显示**,池要 `SeenByDefault => true`(默认 false)。

### 7.5 自定义池怎么上图鉴(PartSmith 方案)

1. **池定义**:`CustomCardPoolModel` 子类 + `IsShared => true`(才进 `ModelDb.AllCardPools`;否则 `card.Pool` 抛 "not in any card pool")+ `SeenByDefault => true`。卡片加 `[Pool(typeof(该池))]` → `c.Pool` 运行时指向该池。
2. **池过滤器按钮**(`PartSmithCardLibraryFiltersPatch`,postfix `NCardLibrary._Ready`,**`Priority.First`**):
   - 原版只给「角色」生成自定义池按钮(BaseLib `CustomPoolFilters` 按 `CustomCharacterModel.CardPool` 过滤)——**共享自定义池默认没有按钮,光靠 ShouldShowInCardLibrary 图鉴里点不到**。
   - 按钮 = `NCardPoolFilter`(64x64)+ 纯色块 `ImageTexture`(56x56,@(4,4),scale0.9,pivot28,材质 `ShaderUtils.GenerateHsv(1,1,1)` 每实例独立)+ Shadow(black a=0.25,ShowBehindParent)+ `%SelectionReticle`(`ui/selection_reticle` 场景);`AddSibling` 挂网格 + `_poolFilters.Add(filter, 谓词 c.Pool is XxxPool)` + `Toggled` 反射调私有 `UpdateCardPoolFilter(filter)` + `FocusEntered` 写 `_lastHoveredControl`。
   - 默认选中重指:`_cardPoolFilters[ModelDb.Character<BigWarrior>()] = 费用按钮`(局内打开图鉴默认费用卡池;效果卡要自己点绿色按钮)。
   - **Harmony 后置补丁顺序实证**:`PatchSorter` 用 `-priority.CompareTo` 降序 → **postfix 高优先先跑**;想先于 BaseLib `AdjustFilterScales`(Normal)插入按钮用 `Priority.First`(不是 Last)。
3. **图鉴显示正确性 = 数据层三条件**:`ShouldShowInCardLibrary=true` + 池谓词命中(`c.Pool` 类型) + unlocked/seen 满足。

### 7.6 自检命令 `parttest library [effect|cost|hunter_effect|hunter_cost|all]`(2026-08-10)

镜像 §7.2+§7.4 逻辑,输出各池在图鉴里**应显示**的卡及可见状态(`LOCKED`/`NOT_SEEN`/`VISIBLE`),等价于图鉴点该池按钮后的可见集:

```
parttest library effect →
  AllCards=801 ShouldShowInCardLibrary=799
  [PartSmithEffectCardPool] matching=89 SeenByDefault=True
    PARTSMITH-STOMP_FRAGMENT | 践踏 | Attack/Common | pool=PartSmithEffectCardPool | VISIBLE
```

- 排查"某张卡图鉴里不显示"先跑它:在列表 = 数据层 OK(收卡/池/可见性都对);不在列表 = 收卡/池条件不满足;在但 `LOCKED`/`NOT_SEEN` = 解锁/已见问题。
- **实测(践踏/惊逃改名后)**:STOMP/STAMPEDE 均 `VISIBLE`,matching=89(=84 效果卡 + 5 张 demo),数据层无问题。**若屏幕上看不到但仍 VISIBLE → 属 UI 层**(默认选中费用池、按钮交互、网格渲染),不是收卡逻辑——按 7.3 默认选中池那步排查。

---

## 8. 多人同步(PartSmith 自研,已验证 + 实施 2026-08-10)

> StS2 多人 = **确定性复算**:所有机器按同一消息顺序各自复算,主机是校验和权威。
> 本 mod 的"多人同步拼接操作"架构 = **同步玩家选择(哪张效果卡/目标卡)+ 全机器确定性执行 mutation**,
> **不需要自定义 GameAction/INetAction**(那是给"无同步选择上下文"的裸 input 用的;mod 注册自定义 INetAction 原生支持,
> 见 `ActionTypes.Initialize` = `INetActionSubtypes.All + ReflectionHelper.GetSubtypesInMods<INetAction>()`)。

### 8.1 三条已验证的事实(决定了"该怎么做")

1. **奖励/篝火的 OnSelect 在每台机器上都跑**:
   - 奖励:`RewardsSetSynchronizer.HandleRewardSelectedMessage`(注册在 `RunLocationTargetedMessageBuffer`)→ `SelectRewardForPlayer` → `reward.SelectUnsynchronized()` → `OnSelect()`。所有机器收到同一 `RewardSelectedMessage`,在同一 SpliceReward 实例上跑 OnSelect。
   - 篝火:`RestSiteSynchronizer.ChooseOption`(处理 `OptionIndexChosenMessage`)→ `option.OnSelect()`。所有机器跑。
2. **`CardSelectCmd.FromDeckGeneric` / `FromSimpleGrid` 内部已自带对称 reserve**(`CardSelectCmd.cs:781-799 / 399-434`):全机器 `ReserveChoiceId(player)` → owner 弹 UI `SyncLocalChoice` / 远端 `WaitForRemoteChoice`。owner 侧 `ShouldSelectLocalCard(player)` = `LocalContext.IsMe(player) && NetType != Replay`。
3. **远端拿到的目标卡是卡组里的真实实例**:choice 结果按 `PlayerChoiceType.DeckCard` 序列化 → `NetDeckCard.ToCardModel(player)` = `player.Deck.Cards[DeckIndex]`(`NetDeckCard.cs:36`)。对它的 mutation(`CardModifier.AddModifier`,纯内存 `InsertSorted`,无网络副作用)在每台机器上对同一张卡复算 → 状态一致。

### 8.2 反模式(曾经的真实 bug,2026-08-10 已修)

```csharp
// ❌ 错误:远端提前 return,不做 reserve
protected override async Task<bool> OnSelect() {
    if (!LocalContext.IsMe(Player)) return false;   // 远端少 reserve → 计数器永久漂移
    ...
    var target = (await CardSelectCmd.FromDeckGeneric(...)).FirstOrDefault(); // owner 才 reserve
    ...
}
```
后果:`PlayerChoiceSynchronizer._choiceIds[该玩家]` 跨机差 N(=拼接次数);`NetFullCombatState.FromRun` **每次校验都哈希 `nextChoiceIds`**(`NetFullCombatState.cs:393,469`)→ 拼接后每次进战斗(`After player turn start`)/出篝火/出事件都报 divergence,直到断线。

### 8.3 正确样板(镜像原版 `CardReward.OnSelect` 的 reserve 段,`CardReward.cs:183-228`)

```csharp
// OnSelect 在每台机器上跑。所有"需要玩家选"的步骤都必须:全机 reserve → owner Sync / 远端 Wait。
var synchronizer = RunManager.Instance.PlayerChoiceSynchronizer;
var rewardOptions = CardRewardAlternative.Generate(this);   // ⚠ 全机无条件调用(内部跑 Hook)
NCardRewardSelectionScreen? screen = null;
if (LocalContext.IsMe(Player)) screen = NCardRewardSelectionScreen.ShowScreen(cards..., rewardOptions);
int? selected = null;
uint choiceId = synchronizer.ReserveChoiceId(Player);        // 全机同一 ID
if (LocalContext.IsMe(Player)) {
    if (screen != null) { try { selected = await screen.OptionSelected(); } catch (TaskCanceledException) { selected = null; } NOverlayStack.Instance?.Remove(screen); }
    synchronizer.SyncLocalChoice(Player, choiceId, PlayerChoiceResult.FromIndex(selected));
} else {
    selected = (await synchronizer.WaitForRemoteChoice(Player, choiceId)).AsIndexOrNull();
}
if (selected == null) return false;
// 之后的 mutation(AttachEffect/DetachAllEffects)全机对同步过的同一(目标卡,效果卡)复算,无需再包 GameAction。
```

### 8.4 注意事项

- **`NCardRewardSelectionScreen` 不自己 reserve**(已 grep 确认),reserve 是调用方职责;别重复 reserve。
- **跳过/关闭也要同步**:任何分支退出前都要保证"该 reserve 的都 reserve 了 + 结果已 SyncLocalChoice",否则计数器仍然漂移。
- **toast 等纯 UI 只在 owner 显示**(`if (LocalContext.IsMe(...))`);日志同理(避免远端刷无关日志)。
- 自定义 `GameAction`+`INetAction`(序列化字段 `IPacketSerializable.Serialize/Deserialize`,示例 `NetDiscardPotionGameAction`)仅在 mutation 不在同步选择之后时才需要——目前拼接场景不需要。
