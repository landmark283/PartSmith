## v0.1.1 三个问题修复(2026-08-13,方案见 `v0.1.1修改方案.md`,已构建部署)

**1. 百科全书不显示效果卡升级数值**
根因:图鉴升级预览走原版 `CardModel.GetDescriptionForUpgradePreview`,而升级数值抬升只在拼接时由 `RefreshPreviewForHost`(需宿主)执行,裸效果卡数值停在基础值。修复:`Patches/EffectCardUpgradePreviewPatch.cs` prefix hook `GetDescriptionForUpgradePreview`,当被渲染的是裸效果卡时 `effect.RefreshPreviewForHost(effect, null)`(图鉴 inspect 已 MutableClone+UpgradeInternal,宿主=自己即取到升级级数)。纯展示,不改 BaseValue。

**2. 储君效果卡不显示所需星辉**
根因:原版 NCard 星费节点(`%StarLabel/%StarIcon`)按 `GetStarCostWithModifiers()` 显示,效果卡是 canonical 单例、该值恒 -1 → 星费被隐藏。修复:`Patches/NCardStarCostLabelPatch.cs` postfix `NCard.UpdateVisuals`,Model 是裸效果卡时点亮原版星节点、填 `effect.StarCost`(储君 20 张 StarCost>0 生效,其余角色恒 0 自动隐藏)。NCard 是所有卡面渲染共用节点 → 覆盖奖励 3 选 1/图鉴/检视/篝火/拼卡选牌全部界面。

**3. 拼接效果缺失(群攻在前 + 单目标在后)**
根因:`CostCardModelBase.TargetType` 取首个非 None/Self 效果的 TargetType,群攻(AllEnemies)在前 → 宿主整体变群攻 → 基游戏不弹单目标选择 → 后方 152 张单目标效果卡(AnyEnemy 146 + AnyAlly 6)的 `cardPlay.Target` 为 null → 全被 `if (cardPlay.Target == null) return;` 静默跳过。修复(方案2,`EffectAttachmentModifier.OnPlay` 加 `EnsureTargetFallback`):按效果阵营校验——当前 target 阵营不符时,从 `CombatState.HittableEnemies` / `PlayerCreatures.Where(IsAlive)` 用确定性 Rng(`RunState.Rng.CombatTargets`,多人联机跨机一致)随机补一个,反射写 `cardPlay.Target`(`<Target>k__BackingField`,required init 构造后不可改)。同阵营后续效果共用该目标(如 Twin Strike 打同一敌人),换阵营重新随机。群攻/随机/自身等不依赖 Target 的类型不兜底。1 处改动覆盖全部 152 张,无需逐卡改。

---

### 潜伏 X 费 bug:5 张 X 费效果卡修复 + 简化卡恢复含 X(2026-08-12 发现并修复)

**现象**:拼了猎人 `Skewer`、猎人 `Malaise`、王 `HeavenlyDrill` 效果的拼卡打出 → 抛异常(代码路径已确认)。

**根因**:这三张是原版 `HasEnergyCostX`(X 费)卡,生成的 Fragment 在 `ExecuteEffect` 里机械照抄了 `ResolveEnergyXValue()`。但效果卡单例构造 `base(0, ...)` 不带 X 费、生成的 Fragment **没覆写 `HasEnergyCostX`** → `this.EnergyCost.CostsX == false` → `ResolveEnergyXValue()`(`CardModel.cs:1123`)直接 `throw new InvalidOperationException("This card does not have an X-cost.")`。效果卡不在战斗牌堆,`CombatState`/`CapturedXValue` 都为空,这个调用永远走 throw 分支。

**同族排查**:5 角色池全量查 `HasEnergyCostX => true`,共 9 张(Volley 是无色卡不入池)。除上述 3 张崩外,骨头人 `Dirge`/`Eradicate` 被生成器**简化**成固定值(召唤 2 次 / 命中 3 次),与 X 语义不符。机器人 `MultiCast`/`Tempest` 已在移植时手写 X。

**补移植(2026-08-12 下午)**:大战士 `Whirlwind`(旋风斩)当初与 Cascade 一起作为 X 费被生成器排除、未移植。现按 X 费方案全部补齐:
- `WhirlwindFragment`(点数 1,AllEnemies 命中 X 次,5 伤升级 +3,带原版横向斩击 vfx+sfx,`PortraitSourceCard = Whirlwind`)。**已验证打出正常**。
- `CascadeFragment`(点数 1,打出抽牌堆顶 X{升级+1} 张,`CardPileCmd.AutoPlayFromDrawPile`)。
- `SearingBlowFragment`(灼热打击,点数 6):**自制卡**(原版 StS2 无 SearingBlow),可无限次升级,伤害非线性成长 `damage(L)=12+3L+L(L+1)/2`(每级增量 +4,+5,+6…);`MaxUpgradeLevels=int.MaxValue` 吃满宿主升级级数;覆写 `RefreshPreviewForHost` 显示当前级数真实伤害(+力量/易伤钩子,diff 基线=当前伤害不重复绿);卡面临时借 AshenStrike,待 #26 自定义卡图。

**修复(2026-08-12,已构建部署)**:
- 新建 `Cards/Base/XCostHelper.cs`:`ResolveAndSpend(cardPlay)` = 快照 `Energy` → `PlayerCmd.LoseEnergy(X)` 全花 → 返回 X。5 张共用。
- `Skewer`(命中 X 次)/ `Malaise`(力量−X 虚弱 X,升级 X+1)/ `HeavenlyDrill`(命中 X,≥4 翻倍)→ 改调 `XCostHelper.ResolveAndSpend`,不再调 `ResolveEnergyXValue`。
- `Dirge` 恢复:召唤 X 次 + X 灵魂(原固定 2);`Eradicate` 恢复:命中 X 次(原固定 3),eng/zhs 描述补 "X 次"。
- **7 张 X 费效果卡点数一律 = 1**(MultiCast/Tempest 也归 1);升级语义不动(单级 `IsUpgraded`,+2/+3 同 +1,各卡保留原版伤害/召唤+X+1)。
- 同步 5 个生成 JSON(`partsmith_*_effect_cards.json`)pointCost=1,防再生成回退。

**语义**:X = 打出时的当前能量(宿主壳费用已扣),立刻全花 → 对应原版 X 费"全花剩余能量"。已知取舍:宿主壳非 X 费,0 能量仍可打出 → X=0 空打(无灰显)。

---

## 机器人(Defect fork)移植完成(2026-08-12)

完整移植见 `robot_recreation_plan.md`。要点:角色「机器人」(PlaceholderID="defect",BaseOrbSlotCount=3 继承充能球体系)+ 机器人专属池(defect 蓝图标)+ 17 共享壳瘦子 + 8 专属壳 + **87 张效果卡**(91−4 Basic)。规避/修正的坑:
- **CalculatedVar 单例陷阱**(Barrage/CompileDriver/FlakCannon/HelixDrill/Synchronize/Voltaic):效果卡单例 CombatState null → CalculatedX 恒 0 → 手写从宿主上下文算 + 剥 Calculated* var + 描述去 `{InCombat:\n(...)|}` 行。
- **X 费陷阱**(MultiCast/Tempest):见上,手写全花当前能量,不调 ResolveEnergyXValue。
- **自克隆陷阱**(AdaptiveStrike):克隆宿主拼卡 `cardPlay.Card.CreateClone()`,不克隆效果卡单例。
- **成长卡**(Claw/GeneticAlgorithm):新写 `ClawExtraModifier`/`GenAlgoExtraModifier`(挂宿主拼卡实例,不存效果卡单例)。
- **选牌界面**(Hologram):补 `PARTSMITH-HOLOGRAM_FRAGMENT.selectionScreenPrompt` 本地化键。
- **System 类型名冲突**(Buffer/Turbo):`Buffer`/`Void` 与 System.Buffer/System.Void 冲突(CS0104),全限定 `global::MegaCrit.Sts2.Core.Models.Cards.X`。
- **私人字段**(Ftl `CanDrawCard`):手动内联。
- **升级去除关键词不迁移**(Chill/EchoForm/Fusion/Hologram/Hotfix/Ignition/Rainbow/Voltaic 升级去 Exhaust/Ethereal):拼卡体系只支持升级加关键词(`UpgradeKeyword`),与猎人 DemonicShield 同款已知偏差。

---

## 吊杀(Hang)拼卡伤害不乘层数 + 行动缓慢改为消耗制(2026-08-11)

### 1. 吊杀:拼卡打出的伤害不乘吊杀层数(钩子失效)

**现象**:拼了「吊杀」效果的费用卡打出去,伤害始终是基础值(10/升级 13),不随目标身上吊杀层数翻倍。

**根因**:原版 `HangPower.ModifyDamageMultiplicative`(`.tools/decompiled/sts2/MegaCrit.Sts2.Core.Models.Powers.HangPower.cs`)只对 `cardSource is Hang` 生效(即只有**原版 Hang 卡本体**打出的伤害才被乘层数)。我们的拼卡宿主是 `CostCardModelBase` 子类,不是 `Hang` → 钩子对该伤害返回 1(无增益)。`DamageCmd` 的 multiplicative hook(`Hook.cs:2539`, `num *= item2.ModifyDamageMultiplicative(...)`)确实跑了,只是 HangPower 不认拼卡来源。

**修复**(`HangFragment.ExecuteEffect`,0 行 hooks):手动乘 `dmg = 基础伤害 × Math.Max(1, 目标吊杀层数)`。首打目标无 HangPower(层数 0)→ ×1 = 基础伤害,与原版一致;之后 × 当前层数(层数本身按原版逻辑每次翻倍叠加)。因为拼卡来源不是 `Hang`,基游戏 Hook 不会二次乘 → 无重复加成。

**预览**(2026-08-11 补上):override `HangFragment.RefreshPreviewForHost`,在 base 算完预览(力量/易伤/虚弱/升级)后,若选中目标且有吊杀层数,把 `DynamicVars.Damage.PreviewValue ×= 层数` → 拼卡描述 `{Damage}` 悬停目标时显示真实伤害(如 10→20→40…);未选中目标/目标无吊杀 → 显示基础值。base 每次把 PreviewValue 重置回 BaseValue,不跨宿主累积。(同 Stomp `{CalculatedDamage}` 的预览限制仍然只影响 Stomp 自身——它的倍数来自"本回合已打攻击数",PreviewValue 只显示基础值。)

### 2. 行动缓慢:从"每次 -Amount 永久"改为"按抽牌消耗层数"

**用户规则**:每次抽牌:
- 层数 < 抽牌数 → 实际抽数 = 抽牌数 − 层数,层数清 0。
- 层数 ≥ 抽牌数 → 不抽牌,层数 −= 抽牌数。

即 `consumed = min(层数, 抽数)`,每抽 1 张牌花 1 层慢,不够扣就不抽。

**改动**(`SlowdownPower` + `DrawSlowdownPrefixPatch`):
- `SlowdownPower.Consume(decimal count)`:统一公式,返回实际抽数;内部 `SetAmount` 减层数,减到 0 时 `RemoveInternal()` 自我移除(不残留 0 层图标)。
- `ModifyHandDraw` → 调 `Consume`,管**回合开始抽牌**。
- `DrawSlowdownPrefixPatch`(Harmony prefix, `fromHandDraw=false` 的牌效抽牌)→ 调 `Consume`;`fromHandDraw=true` 已在 `ModifyHandDraw` 消耗 → 跳过防双扣。
- powers.json eng/zhs 的 `PARTSMITH-SLOWDOWN_POWER.description/.smartDescription` 同步改为消耗制文案。

> 自然结果:回合开始抽 5 会先消耗 5 层;层数 ≤5 时一张牌效抽牌都享受不到(已被回合开始抽完),层数 >5 才有余量给牌效抽牌。这就是"每次抽牌消耗"的语义,无需额外处理。

---

## Snap 等 6 张效果卡缺 `selectionScreenPrompt` 键,打出即崩(2026-08-11)

### 现象

拼了「响指」(Snap)效果的费用卡打出后:

```
[ERROR] System.InvalidOperationException: No selection screen prompt for CARD.PARTSMITH-SNAP_FRAGMENT.
   at PartSmith.PartSmithCode.Cards.EffectCards.SnapFragment.ExecuteEffect(...) in .\PartSmithCode\Cards\EffectCards\SnapFragment.cs:line 55
...
[ERROR] GameAction PlayCardAction card: CARD.PARTSMITH-BONE_STRENGTH_COST_SHELL ... completed with exception:
        System.AggregateException: ... (No selection screen prompt for CARD.PARTSMITH-SNAP_FRAGMENT.)
```

(日志:`%APPDATA%\SlayTheSpire2\logs\godot2026-08-11T22.48.43.log`)

### 根因

与 08-10 头槌**同一类** bug:`CardModel.SelectionScreenPrompt` getter 要求本卡 Id 的 `cards.PARTSMITH-XXX_FRAGMENT.selectionScreenPrompt` 本地化键存在,缺键直接 throw(`No selection screen prompt for {id}`)。`SnapFragment.cs:55` 用 `new CardSelectorPrefs(base.SelectionScreenPrompt, 1)` → 选牌界面还没弹出来就崩,「给手牌添加保留」一步完全无法执行。

### 排查:缺键的不止 Snap 一张

全量扫所有用 `base.SelectionScreenPrompt` 的效果卡(`CardSelectCmd.FromHand / FromCombatPile`),共 **6 张缺键**、都会崩:

| 效果卡 | 原版镜像 | 修复用的提示(eng / zhs,复用原版原文) |
|---|---|---|
| `SnapFragment` | Snap | Select a card to add [gold]Retain[/gold] to. / 选择要添加[gold]保留[/gold]的卡牌。 |
| `DredgeFragment` | Dredge | Choose {Amount} {Amount:plural:card\|cards} to put into your [gold]Hand[/gold] / 选择{Amount}张牌加入你的[gold]手牌[/gold]。 |
| `GraveblastFragment` | Graveblast | Choose a card to put back in your Hand. / 选择一张牌放入你的手牌。 |
| `HandTrickFragment` | HandTrick | Choose a card to add [gold]Sly[/gold] to. / 选择一张牌添加[gold]奇巧[/gold]。 |
| `NightmareFragment` | Nightmare | Choose a Card. / 选择一张牌。 |
| `SculptingStrikeFragment` | SculptingStrike | Choose a card to add [gold]Ethereal[/gold]. / 选择一张牌添加[gold]虚无[/gold]。 |

(08-10 只补了 `HeadbuttFragment` 一张;其余 8 张当时已有键。这次把漏的 6 张全补齐。)

### 修复(2026-08-11,已部署)

- **纯数据修复,零 C# 改动**:`eng/cards.json` + `zhs/cards.json` 各补 6 行 `PARTSMITH-X_FRAGMENT.selectionScreenPrompt`,提示文本直接复用原版卡的选牌提示(从 `SlayTheSpire2.pck` 提取,措辞与游戏一致)。
- 重新导出 pck(本地化必须进 pck),已确认 6 个新键在 pck 内;dll/pck 已部署到 `D:/Steam/steamapps/common/Slay the Spire 2/mods/PartSmith/`(22:54)。
- 至此 14 张用 `base.SelectionScreenPrompt` 的效果卡**全部有键**,无一缺键。

### 待验证

游戏内拼上述 6 张效果卡打出 → 选牌界面正常弹出;日志无 `No selection screen prompt`。

### 后续规范

- 新增效果卡若用 `CardSelectCmd.FromHand / FromCombatPile / FromPile` 且提示走 `base.SelectionScreenPrompt`,**必须**同步补 `PARTSMITH-XXX_FRAGMENT.selectionScreenPrompt`(eng+zhs),否则打出即崩。
- 提示文本优先复用原版卡的选牌提示(从 pck 提取),与游戏措辞一致。

---

打出拼了「头槌」效果的费用卡(卡组实例 `CARD.PARTSMITH-WHET_SHELL`,显示名「头槌+」)时:

```
[ERROR] System.InvalidOperationException: No selection screen prompt for CARD.PARTSMITH-HEADBUTT_FRAGMENT.
   at MegaCrit.Sts2.Core.Models.CardModel.get_SelectionScreenPrompt()
   at PartSmith.PartSmithCode.Cards.EffectCards.HeadbuttFragment.ExecuteEffect(...) in .\PartSmithCode\Cards\EffectCards\HeadbuttFragment.cs:line 49
   at PartSmith.PartSmithCode.Cards.Base.EffectAttachmentModifier.OnPlay(...) in .\PartSmithCode\Cards\Base\EffectAttachmentModifier.cs:line 108
...
[ERROR] GameAction PlayCardAction ... completed with exception: System.AggregateException:
        (No selection screen prompt for CARD.PARTSMITH-HEADBUTT_FRAGMENT.)
```

(日志文件:`%APPDATA%\SlayTheSpire2\logs\godot2026-08-10T14.01.16.log`,15:52:24附近。)

### 2.1 抛异常的位置 = 选牌界面的提示文案,不是牌堆操作

`HeadbuttFragment.ExecuteEffect` 第 49 行:

```csharp
CardModel cardModel = (await CardSelectCmd.FromCombatPile(
    prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1),   // ← 崩在这一行
    context: choiceContext, pile: PileType.Discard.GetPile(cardPlay.Player), player: cardPlay.Player))
    .FirstOrDefault();
```

### 2.2 `SelectionScreenPrompt` getter 会在缺本地化键时直接 throw

反编译 `CardModel`(`.tools/decompiled/sts2/MegaCrit.Sts2.Core.Models.CardModel.cs:129`):

```csharp
protected LocString SelectionScreenPrompt
{
    get
    {
        LocString locString = new LocString("cards", base.Id.Entry + ".selectionScreenPrompt");
        if (!locString.Exists())
        {
            throw new InvalidOperationException($"No selection screen prompt for {base.Id}.");
        }
        DynamicVars.AddTo(locString);
        return locString;
    }
}
```

后续同类风险提醒

- 今后新增效果卡若用到 `CardSelectCmd.FromCombatPile / FromHand / FromPile` 等,记得同步补 `PARTSMITH-XXX_FRAGMENT.selectionScreenPrompt` 本地化键(或复用静态 prompt)。
- 原版还有多张带选牌界面的卡(`Dredge`/`DualWield`/`SecretTechnique`/`SecretWeapon`/`ThinkingAhead`/`Transfigure`/`Wish` 等),目前都不在 84 张效果卡内,本次无需处理。

---

## 践踏改名 / 惊逃移植验证中发现的两件事(2026-08-10)

### 1. `parttest make` 的 effectId 必须带全 `PARTSMITH-` 前缀

- **现象**:`parttest make STOMP_FRAGMENT` → `Effect card 'STOMP_FRAGMENT' not found. Available: PARTSMITH-ABRASIVE_FRAGMENT, ...`
- **根因**:`PartSelfTestCommand.Make` 用**精确匹配** `ModelDb.All.OfType<EffectCardModelBase>().FirstOrDefault(c => c.Id.Entry == id)`(`PartSelfTestCommand.cs:112`),而 `Id.Entry` 含 `PARTSMITH-` 前缀 → 短名永远匹配不上。成本卡同理(`:93`)。
- **修复/规范**:命令行参数传全名,如 `parttest make PARTSMITH-STOMP_FRAGMENT`。`verify_in_game.py` 里全部场景已统一改为全名(2026-08-10)。
- **通用规则**:凡是给 mod 命令传卡 id,一律用带 `PARTSMITH-` 前缀的全称(与 `partsplice` 的 effectId 约定一致)。

### 2. 验证脚本的两处"假 FAIL"(测试写法问题,非游戏 bug)

- **践踏 0 攻击 Δ0 测出 Δ4**:`CombatManager.History.CardPlaysFinished` 数的是"本回合已完成的攻击牌"。若复用**上一轮测试的同一回合**(该回合已打过一张践踏——践踏 type=Attack 也算攻击),再打践踏时 attacksPlayed=1 → 伤害 0+4=4。**不是自计数 bug 复发**(`e.CardPlay != cardPlay` 修复仍有效),是测试回合被污染。**规范:测 0 攻击 Δ0 前强制新回合/新战斗**(`parttest room` 每次强制新战斗最干净)。
- **惊逃回合末自动打 Δ0**:`StampedePower` 自动打的是**随机敌人**(每层从手牌随机抽一张攻击 `CardCmd.AutoPlay`),3 敌中只打 1 个 → 其余 2 敌 Δ0 属正常。**规范:随机目标场景断言"至少一个敌人掉血"(合计 Δ>0),不能逐个敌人断言 Δ>0**。