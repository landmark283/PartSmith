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