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