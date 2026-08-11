# Debug:头槌(Headbutt)拼卡效果抛异常诊断与修复

> 日期:2026-08-10
> 对象:`HeadbuttFragment`(效果卡),拼卡机制(`EffectAttachmentModifier` / `CostCardModelBase`)
> 结论先行:**与"拼接卡类型不同"无关**。异常发生在**选牌界面创建阶段**,根因是效果卡缺少 `selectionScreenPrompt` 本地化键。修复 = 补 2 行本地化(数据),**无需改任何 C# 代码**。

---

## 1. 现象(游戏日志原文)

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

(日志文件:`%APPDATA%\SlayTheSpire2\logs\godot2026-08-10T14.01.16.log`,15:52:24 附近。)

影响:整张拼卡的 `PlayCardAction` 以异常结束 —— 头槌的 9 点伤害已打出,但「从弃牌堆选一张牌放到抽牌堆顶」这一步**完全无法执行**(选牌界面还没弹出来就崩了)。

---

## 2. 根因(已从反编译源码核实)

用户最初的怀疑是「拼接卡类型与普通卡不同、头槌脚本无法操作拼接卡」。**实际不是**。逐层定位:

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

- 原版 `Headbutt`:`Id.Entry = "HEADBUTT"`,游戏 pck 里有 `cards.HEADBUTT.selectionScreenPrompt` 键 → 不崩。
- 我们的效果卡 `HeadbuttFragment`:`Id.Entry = "PARTSMITH-HEADBUTT_FRAGMENT"`,而 mod 的本地化里**没有** `cards.PARTSMITH-HEADBUTT_FRAGMENT.selectionScreenPrompt` 键 → getter 抛 `InvalidOperationException`。

### 2.3 「把牌放到抽牌堆顶」这一步没问题

`CardPileCmd.Add(CardModel card, PileType, CardPilePosition)`(`.tools/decompiled/sts2/MegaCrit.Sts2.Core.Commands.CardPileCmd.cs:320`)接受任意 `CardModel`,无类型限制。拼卡本来就是正常的 CardModel(会进手牌/弃牌堆/抽牌堆),牌堆移动对它完全适用。这段代码**根本没有执行到**。

---

## 3. 冲突点与影响范围

**冲突点**:`HeadbuttFragment.cs:49` 用了 `base.SelectionScreenPrompt`,它要求"本卡 Id 的 selectionScreenPrompt 本地化键存在"。

**已排查全部 84 张效果卡,只有 HeadbuttFragment 踩中**:

| 效果卡 | 选牌命令 | 提示来源 | 是否安全 |
|---|---|---|---|
| `HeadbuttFragment` | `CardSelectCmd.FromCombatPile` | `base.SelectionScreenPrompt`(**本卡键**) | ✗ 缺键 → 崩 |
| `ArmamentsFragment` | `CardSelectCmd.FromHandForUpgrade` | 内部静态 `gameplay_ui.CHOOSE_CARD_UPGRADE_HEADER` | ✓ 永远存在 |
| `BrandFragment` | `CardSelectCmd.FromHand` | `CardSelectorPrefs.ExhaustSelectionPrompt`(= `card_selection.TO_EXHAUST`) | ✓ 永远存在 |
| `BurningPactFragment` | `CardSelectCmd.FromHand` | 同上 | ✓ 永远存在 |
| `TrueGritFragment` | `CardSelectCmd.FromHand` | 同上 | ✓ 永远存在 |

另外:`SelectionScreenPrompt` 是 `protected` **非 virtual** getter,无法在效果卡里 override,所以"补本地化键"是唯一可行路径(而不是覆写属性)。

---

## 4. 修复方案(推荐:补本地化键,零代码改动)

### 4.1 在两个 cards.json 补键

**`src/PartSmith/PartSmith/localization/eng/cards.json`**(在 `PARTSMITH-HEADBUTT_FRAGMENT.description` 之后插入):

```json
"PARTSMITH-HEADBUTT_FRAGMENT.selectionScreenPrompt": "Choose a card to put on top of your Draw Pile.",
```

**`src/PartSmith/PartSmith/localization/zhs/cards.json`**(在 `PARTSMITH-HEADBUTT_FRAGMENT.description` 之后插入):

```json
"PARTSMITH-HEADBUTT_FRAGMENT.selectionScreenPrompt": "选择一张牌放到你的抽牌堆顶。",
```

> 两行文本**直接复用原版** `HEADBUTT.selectionScreenPrompt` 的 eng/zhs 原文(从 `SlayTheSpire2.pck` 提取,保证与原版选牌提示一致;eng = "Choose a card to put on top of your Draw Pile.",zhs = "选择一张牌放到你的抽牌堆顶。")。

### 4.2 重新打包(本地化必须进 pck)

- 改完本地化后需**重新导出 mod pck**,让 `PartSmith.pck` 里的 `localization/eng/cards.json`、`localization/zhs/cards.json` 带上新键。
- 已核实:当前构建产物 `src/PartSmith/dist/PartSmith/PartSmith.pck` 中 `selectionScreenPrompt` 出现次数为 **0**(而 `PARTSMITH-HEADBUTT_FRAGMENT.title` 已在内),即现在这个 pck 打上去必然崩。
- 部署前关闭游戏;部署后由你手动启动验证。

### 4.3 验证步骤

1. 打到/用测试命令拼一张「头槌」效果的费用卡,弃牌堆里留牌。
2. 打出该拼卡:伤害正常 → **弹出选牌界面**(提示:从弃牌堆选择一张牌放到抽牌堆顶)→ 选牌后该牌置于抽牌堆顶、下回合可抽到。
3. 日志里无 `No selection screen prompt for ...` 异常;`PlayCardAction` 正常完成。

---

## 5. 备选兜底(不推荐)

若想完全不依赖本地化键,可在 `HeadbuttFragment.ExecuteEffect` 改用静态提示,例如:

```csharp
var prefs = new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1);
```

但 `card_selection.TO_DISCARD` 措辞偏"弃牌"用途,不够贴切,且与原版「放到抽牌堆顶」提示不一致。**推荐走方案 4.1**(与 title/description 的既有做法一致,也是原版机制)。

---

## 6. 后续同类风险提醒

- 今后新增效果卡若用到 `CardSelectCmd.FromCombatPile / FromHand / FromPile` 等,记得同步补 `PARTSMITH-XXX_FRAGMENT.selectionScreenPrompt` 本地化键(或复用静态 prompt)。
- 原版还有多张带选牌界面的卡(`Dredge`/`DualWield`/`SecretTechnique`/`SecretWeapon`/`ThinkingAhead`/`Transfigure`/`Wish` 等),目前都不在 84 张效果卡内,本次无需处理。

---

## 7. 实施状态(2026-08-10,已全部落地并部署)

方案 4.1 已实施:**eng/zhs cards.json 各补了 1 行 selectionScreenPrompt**,并重新打包 pck(本地化已进 pck,见下)。本次会话同时完成的其它卡片改动(与 Headbutt 同批部署):

| 改动 | 内容 | 实现 |
|---|---|---|
| 头槌 | 补 `PARTSMITH-HEADBUTT_FRAGMENT.selectionScreenPrompt`(eng+zhs) | 本地化 2 行,零代码 |
| 军械(升级描述) | 升级后显示「升级你的所有手牌」 | 描述改 `{IfUpgraded:show:升级你的所有手牌。\|升级你手牌中的一张牌。}`;**新增宿主感知渲染** —— `EffectCardModelBase.BuildEffectDescription`(复刻 `GetDescriptionForPile` 的 LocString 拼接,但 UpgradeDisplay 由调用方显式指定)+ `EffectAttachmentModifier.ModifyDescription` 按宿主 `IsUpgraded`/`UpgradePreviewType` 传入 Upgraded/UpgradePreview/Normal。之前 canonical 效果卡 `IsUpgraded` 恒 false,`{IfUpgraded:show}` 永远渲染基础分支(已知限制),现在拼卡描述正确切换 |
| 踩踏(重做) | 点数 3;对所有敌人造成 base+每张攻击牌 +4 伤害;升级 base 0→4 | `StampedeFragment` 重写:`PointCost=3`、`CardType.Attack`/`Common`/`AllEnemies`、`CanonicalVars = CalculationBaseVar(0)+ExtraDamageVar(4)+CalculatedDamageVar(ValueProp.Move).WithMultiplier(本回合已打出攻击数)`、`GetUpgradeDelta("CalculatedDamage")=>4m`、移除 `UpgradeEnergyGain`;`ExecuteEffect` 用 `CombatManager.Instance.History.CardPlaysFinished` 数本回合已完成的攻击(`HappenedThisTurn`+`Type==Attack`+`Player==owner`,不含正在打出的拼卡本身,同原版 Finisher),伤害=`UpgradedValue(CalculationBase.BaseValue,4m)+ExtraDamage.BaseValue×attacksPlayed`,`DamageCmd.Attack(...).TargetingAllOpponents(cardPlay.Card.CombatState)` |
| 幽冥嚎叫(脚本检查) | 原伤害脚本本身正确(18→升级24,对全敌人);补 Exhaust 关键词 + 消耗后逐回合复播 | `HowlFromBeyondFragment` 加 `CanonicalKeywords => [Exhaust]` + `ReplayWhenExhausted => true`;`CostCardModelBase` override `AfterAutoPostPlayPhaseEntered`(宿主在消耗牌堆且拼了 ReplayWhenExhausted 效果 → `CardCmd.AutoPlay(choiceContext, this, null)`)。关键词由 `SpliceController.AttachEffect` 转移到宿主拼卡 |

**部署验证(2026-08-10 14:32):**
- `dotnet build`:0 错 0 警。
- Godot `--headless --export-pack` 重打 pck(141132B)。
- pck 内已确认含:eng+zhs `selectionScreenPrompt`(计数 2)、军械 `Upgrade all cards in your hand.` / `升级你的所有手牌`、踩踏 `Deal {CalculatedDamage:diff()} damage to ALL enemies` / `本回合每打出一张攻击牌`。
- dll/json/pdb/pck 已复制到 `D:\Steam\steamapps\common\Slay the Spire 2\mods\PartSmith\`(部署前确认游戏已关闭)。
- **待游戏内验证**:头槌选牌界面弹出;升级军械拼卡描述显示「升级你的所有手牌」;踩踏拼卡对全敌人按攻击数加伤、升级后 base 0→4;幽冥嚎叫拼卡打出后进消耗、每回合结束自动复播。

> 已知限制(与 BodySlam/PerfectedStrike 一致):踩踏描述里的 `{CalculatedDamage}` 预览只显示基础值(0/4)+力量/易伤修正,**不**含已打攻击数的加成 —— `CalculatedVar.Calculate` 用 canonical 效果卡的 `CombatState`(null)乘数恒 0;实际伤害由 `ExecuteEffect` 按宿主上下文计算,是准的。

---

## 8. 自动化游戏内验证(2026-08-10,已完成首轮全量回归)

> 目标:每次改卡后能自己在游戏里验证「显示是否正确 / 打出是否有效果」,不依赖人工肉测。
> 方案:sts2-bridge(端口 27100)驱动游戏 + `parttest` 自测命令直达测试房间,桥驱动手动逐步操作。

### 8.1 基础设施(全部落地)

| 组件 | 说明 |
|---|---|
| `parttest room [encounterId]` | 直达测试战斗房间(默认 `KNIGHTS_ELITE`,正好 3 敌)。复用 `fight` 的 `RunManager.EnterRoomDebug`,先退出当前房间再进战斗。 |
| `parttest maxhp` | 战斗激活后,把当前战斗所有敌人 `SetMaxHpInternal(999999999)`(≈int 上限),得到"打不死的测试敌人"。 |
| `parttest make/info/encounters` | 现造拼卡塞手牌 / 打印手牌卡真实渲染信息 / 列出遭遇 id。 |
| **bridge `/console` 主线程修复** | 原 bridge 的 HTTP 服务器跑在 `Task.Run` 后台线程,`/console` 直接反射调 `ProcessCommand`,导致房间切换/场景实例化等 **Godot 节点操作在后台线程执行,时好时坏**(偶发 `NCombatRoom` UI 未加载、`Can't add child 'Card' to 'PlayContainer'`、`SetUpCombat` NRE)。修复:`BridgeGameApi.ExecuteConsoleCommandAsync` 用 `BridgeCoordinator.RunOnMainThreadAsync` 包住 `ProcessCommand`(与 `/action` 一致)。**这是本次所有诡异崩溃的根因**。 |

### 8.2 首轮全量回归结果(干净状态,全部 PASS)

| 卡片 | 验证点 | 结果 |
|---|---|---|
| 军械 Armaments | 基础描述「升级你手牌中的一张牌」→ `upgrade` 后「升级你的**所有**手牌」 | ✅ PASS(用 `parttest info` 读游戏端真实渲染;`/state` 描述不反映升级状态) |
| 头槌 Headbutt | 打出→选牌界面→选弃牌堆一张→该牌到**抽牌堆顶** | ✅ PASS(bridge 自动完成选牌;弃牌堆的打击正确置顶) |
| 践踏 Stomp(重做) | 0 攻击 Δ0 / 打 1 攻击 Δ4 / 升级+1 攻击 Δ8(每敌) | ✅ PASS |
| 幽冥嚎叫 HowlFromBeyond | 打出 Δ18×3→进消耗堆→结束回合→**自动复播 Δ18×3**→重新进消耗堆 | ✅ PASS(修复 bridge 线程后复播正常) |

### 8.3 本次发现并修复的 bug

1. **践踏自计数(已修)**:`StompFragment`(原 `StampedeFragment`)`ExecuteEffect` 的攻击计数把**正在打出的拼卡自己**算进去了 —— 0 攻击首打就 Δ4(应为 Δ0)。`CombatManager.History.CardPlaysFinished` 在执行效果时已含当前 play,修复为 `&& e.CardPlay != cardPlay`(排除自身;其它本回合打出的攻击照常计数)。已在 `StompFragment.cs` 修好并回归验证 Δ0/Δ4/Δ8。

2. **践踏标题 + 卡图错误(已修,2026-08-10)**:基础游戏英文名易混——践踏=**Stomp**(3费攻击,12伤全体,每张攻击牌减1费),惊逃=**Stampede**(2费能力,回合末自动打随机攻击)。原 bug 两层:
   - 标题:`zhs/cards.json` 的 `PARTSMITH-STAMPEDE_FRAGMENT.title` 误写 **"冲锋"**。
   - 卡图:`PortraitSourceCard => ModelDb.Card<Stampede>()` 取到了惊逃的图。
   **修复方案(用户定)**:践踏重做改名 `StompFragment`(id `PARTSMITH-STOMP_FRAGMENT`,卡图 `Card<Stomp>`,标题践踏),删掉忠实原版的 `StompFragment`;惊逃移植成新 `StampedeFragment`(id `PARTSMITH-STAMPEDE_FRAGMENT`,卡图 `Card<Stampede>`,标题惊逃,效果贴近原版 StampedePower + 升级改能量增益)。取卡图逻辑记入 api-notes.md §2.4。

### 8.4 手动驱动流程(供下次用)

1. `python` 起 `D:/Steam/steam.exe -applaunch 2868840`,轮询 `GET /health` + `GET /state`(token 在 `%APPDATA%\SlayTheSpire2\bridge\session.json`)。
2. 主菜单:有存档则 `main_menu:abandon_current_game` → `main_menu:confirm_abandon_run` → `main_menu:singleplayer` → `run_mode:standard` → `character_select:5`(游戏内是 DEFECT;角色与卡测无关)→ `embark`。
3. run 激活后**等 ~10s 让地图/事件房间稳定**,再 `POST /console {"command":"parttest room"}`,轮询 `combat.in_progress`。
4. `parttest maxhp` 把敌人顶到 999999999。
5. 逐个场景:`parttest make <效果卡id>` → `upgrade <手牌idx>` → `/action` 打牌 → 读 `/state`(血量/格挡/牌堆)→ `parttest info` 读真实渲染。测试前 `energy 20`;结束回合用 `end_turn`;保护玩家用 `block 999999 0`。
6. `/console`、`/action` 响应是**扁平** `{ok, output}`(bridge 修复后)。

### 8.5 已知注意事项

- `/state` 的卡描述不反映升级状态(基础值),要看升级后的真实渲染用 `parttest info <handIdx>`。
- `/action` 会**自动完成**单选项的选牌界面(如头槌只剩一张可选时),不会停住等人工。
- 敌人有格挡时伤害先吃格挡,测 Δ 要 `Δ = ΔHP + Δ格挡`。
- 玩家不保护会被骑士打死(game over),测复播前先 `block 999999 0`。
- 桥驱动要**慢**:每条命令后轮询 `/state` 稳定再发下一条;`fight`/`parttest room` 这类房间切换现在主线程执行(已修),但仍是异步的,要等 `combat.in_progress`。
