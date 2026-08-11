ok,接下来是补充费用牌的设计。我的设计是这样的：费用牌共13张，分别是：
1. 0费，3点，普通；
2. 0费，6点，稀有；
3. 0费，6点，扣自己1滴血，罕见；
4. 1费，6点，普通；
5. 1费，10点，罕见；
6. 1费，10点，给选定敌人增加1点力量，罕见；
7. 1费，15点，本回合 自己的力量-1，敏捷-1；，罕见；
8. 1费，15点，自己-3血量，稀有；
9. 2费，15点，稀有；
10. 2费，15点，血量-1，稀有；
11. 2费，20点，自己 力量-1，罕见；
12. 3费，20点，稀有；
13. 3费，30点，虚无，消耗，罕见；
14. 2费，15点，消耗，普通。

# 参考基准
点数仅用于mod PartSmith
'''
1费 6点
2费 10点
3费 15点

罕见牌 +1点
稀有牌 +2点
'''
我们需要一个负面状态，动作缓慢，效果是：用缓慢层数减1来替代下一次抽牌。（比如我有2层缓慢，那么draw 3 就只能抽到1张牌。  缓慢对回合开始时的抽牌也生效）

## 公共池
接下来是正式版本的费用牌设计，首先是所有角色共用的部分：
1. 0费，3点，普通。
2. 0费，6点，稀有。
3. 1费，6点，普通。
4. 1费，10点，稀有。
5. 1费，10点，本回合 自己的力量-1，敏捷-1，普通。
6. 1费，10点，消耗，罕见。
7. 2费，10点，普通。
8. 2费，15点，本回合 自己的力量-1，敏捷-1，普通。
9. 2费，15点，稀有。
10. 3费，18点，普通。
11. 3费，30点，虚无，消耗，稀有。
12. 3费，30点，力量-1，稀有。
13. 1费，15点，自己得到1层动作缓慢，罕见。
14. 2费，20点，自己得到1层动作缓慢，罕见。
15. 1费，10点，丢弃1张牌，稀有。
16. 1费，10点，弃牌堆中增加一张眩晕，罕见。
17. 1费，10点，指定敌人获得2点力量，罕见。

## 战士池
战士的费用卡片由公共池加战士池组成。
1. 0费，6点，-1血量，罕见。
2. 1费，10点，-1血量，罕见。
3. 2费，15点，-1力量，普通。
4. 2费，10点，-1血3次，罕见。
5. 1费，12点，选定敌人获得2层人工制品，稀有。
6. 0费，10点，-1力量 -1血量，稀有。
7. 3费，30点，-2力量，罕见。

## 猎人池
猎人的费用卡片由公共池加猎人池组成。
1. 0费，6点，本回合 自己的力量-1，敏捷-1，罕见。
2. 1费，15点，本回合 自己的力量-1，敏捷-1，稀有。
3. 1费，10点，选定敌人获得2层人工制品，稀有。
4. 0费，15点，+3动作缓慢，稀有。
5. 2费，6点，抽3张牌，罕见。
6. 1费，10点，丢3张牌，罕见。
7. 3费，15点，先抽3张牌，再丢2张，普通。

## 王池
王的费用卡片由公共池加王池组成。
这里的星辉指 王 独有的双费用机制的第二个费用。
1. 0费，3点，+2星辉，稀有。
2. 1费，10点，-1星辉，普通。
3. 1费，6点，+1星辉，罕见。
4. 2费，10点，+3星辉，罕见。
5. 2费，15点，-1星辉，普通。
6. 3费，15点，+5星辉，普通。
7. 1费，15点，-2星辉，稀有。

## 骨头人池
骨头人的费用卡片由公共池加骨头人池组成。
奥提斯是骨头人的召唤物。召唤x，表示如果奥提斯死亡，则让它活，并且增加x生命上限，如果奥提斯活着，则增加它x生命上限。
1. 0费，3点，虚无，普通。
2. 1费，15点，-1力量，罕见。
3. 3费，45点，-3力量，稀有。
4. 1费，12点，奥提斯死去，普通。
5. 0费，10点，-2力量，稀有。
6. 2费，6点，给所有手牌增加虚无，罕见。
7. 1费，6点，召唤3，罕见。

## 机器人池
机器人的费用卡片由公共池加机器人池组成。
1. 0费，6点，向手牌中增加一张伤口，罕见。
2. 0费，6点，本回合 -2敏捷，-2力量，罕见。
3. 0费，3点，触发一个充能球，罕见。
4. 2费，12点，这张牌打出两次，稀有。
5. 1费，10点，像抽牌堆中加入一张眩晕，普通。
6. 1费，15点，冲能球栏位减少1，稀有。
7. 3费，30点，-2力量，冲能球栏位增加1，普通。
8. 0费，4点，罕见。

---
---

# 开发文档(2026-08-11,设计可行性核对)

> 结论先行:**设计里所有状态/效果都做得出来**。常规效果(扣血/属性减/加牌/抽丢牌/星辉/关键字)直接有原版机制或先例可抄;三个"特殊机制"——**动作缓慢**(用户定名 **Slowdown**,永久)、**骨头人的奥提斯召唤**(原版 `OstyCmd` 现成)、**机器人的充能球/打出两次**(原版 `OrbCmd` + `ModifyCardPlayCount` 现成)——也都核实有原生 API。
> **旧 14 壳从角色费用池移除**(用户定:池里不再含旧 14;类直接删除,parttest/partsplice 默认宿主重指新共享#1),每角色费用池 = **新增共享 17(完整,不跳过)+ 角色专属 7 = 24 张**。**机器人池暂缓不做**(角色未做,实现方式留作参考)。
> 用户已确认 5 项(见 §H):「触发一个充能球」=唤起(触发+移除)、「奥提斯死去」已死时空打、机器人暂缓、星辉为 0 可打出、新壳名字随便起。
> **共享牌实现方式(用户定):沿用旧壳的「基类 + 瘦子类」模式**(一张卡类只能归一个池,共享牌每角色写瘦子类保配色)。旧 14 壳从池移除并删除类。

## A. 现状架构(费用牌池怎么工作的)

- 费用卡 = `CostCardModelBase` 子类,**纯 C# 手写**(效果卡是 JSON 生成,费用卡没有生成器)。每张卡声明 `[Pool(某角色费用池)]` → 自动进池(`ModelDb` 反射实例化),**不用在池类里列卡**。
- 每角色 1 个费用池:`PartSmithCostCardPool`(铁甲战士/ironclad紫)、`PartSmithHunterCostCardPool`(猎人/silent绿)、`PartSmithWangCostCardPool`(储君/regent橙)、`PartSmithBoneManCostCardPool`(骨头人/necrobinder粉)。卡面费用图标/描边随池配色。
- 池→奖励:`PartSmithRewardFactory.Create{角色}CostCardReward(player)` 用 `new CardCreationOptions(new[]{ ModelDb.CardPool<池>() }, Encounter, EliteEncounter)` 出 3 选 1(`RewardInjectionPatch` 按当前角色分派到对应池)。
  → **往池里加卡,奖励自动出;奖励代码一行不用改。用户的「只改每个角色的费用牌池」方案成立。**
- ⚠ 一张卡类只能归一个池(`[Pool]` AllowMultiple=false)。所以**共享牌按用户定:沿用旧壳的「基类 + 瘦子类」模式**——战士池写基类壳(效果/点数/稀有度全在基类),猎人/王/骨头人各写瘦子类 `XxxShellHunter/Wang/BoneMan : XxxShell` 只覆写 `[Pool]`(照 `HunterCostShells.cs` 现有瘦子类模式),改效果只改基类一处,后续好改。

## B. 逐项状态/效果可行性(全部反编译核实,含出处)

### B1. 公共/战士/猎人/王池用到的效果(上次已核实)

| 设计里的效果 | 可行性 | 实现方式(API + 出处) |
|---|---|---|
| 扣自己血量(-1/-3) | ✅ | `CreatureCmd.Damage(ctx, Owner.Creature, n, Unblockable\|Unpowered\|Move, this, cardPlay)` —— 抄 `BloodShell.cs` |
| -1 血 × 3 次 | ✅ | 循环 3 次上述 Damage(各 1m),或一次 `Damage(3m)` |
| 本回合力量-1 / 敏捷-1 | ✅ **已有实现** | `PowerCmd.Apply<TempStrDown>(ctx, Owner.Creature, n, Owner.Creature, this)` + `TempDexDown`。抄 `CrumblingShell.cs` + `Powers/CrumblingShell{Strength,Dexterity}DownPower.cs` |
| 永久力量 -1 | ✅ **已有实现** | `PowerCmd.Apply<StrengthPower>(ctx, Owner.Creature, -n, Owner.Creature, this)` —— 抄 `DeadweightShell.cs` |
| 指定敌人 +力量 | ✅ **已有实现** | `PowerCmd.Apply<StrengthPower>(ctx, cardPlay.Target, n, Owner.Creature, this)` —— 抄 `WhetShell.cs` |
| 指定敌人 +人工制品 | ✅ | `PowerCmd.Apply<ArtifactPower>(ctx, cardPlay.Target, n, Owner.Creature, this)`。`ArtifactPower.cs` 只在"目标==Owner"时挡 debuff → 给敌人挂上会吞玩家对该敌人的下一次 debuff,正是要的负面 |
| 弃牌堆加 1 张眩晕 | ✅ 有先例 | `CombatState.CreateCard<Dazed>(Owner)` + `CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, Owner)` + `CardCmd.PreviewCardPileAdd(...)` —— 抄 `BoostAway.cs` |
| 抽 N 张牌 | ✅ | `CardPileCmd.Draw(ctx, n, Player)`(OnPlay 里 `base.Owner` 就是 Player) |
| 丢弃 N 张牌 | ✅ | `CardSelectCmd.FromHand(ctx, player, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, n, n), null, this)` → `CardCmd.Discard(ctx, cards)`。提示键 `card_selection.TO_DISCARD` 游戏自带 |
| 先抽 3 再丢 2 | ✅ | 上面两行拼一起:先 Draw 3,再从手牌 FromHand(2,2)→ Discard |
| 星辉 ±N(王) | ✅ 已核实 | `PlayerCmd.GainStars(n, player)` / `PlayerCmd.LoseStars(n, player)`(角色无关;只 clamp ≥0,无上限)。**星辉为 0 打「-星辉」壳照常可打**(用户确认),`LoseStars` clamp 0 不报错 |
| 虚无 / 消耗 关键字 | ✅ **已有实现** | 覆写 `CanonicalKeywords`(含 `CardKeyword.Ethereal`)/ `Exhaust` 属性 —— 抄 `VoidShell.cs` |
| **动作缓慢(减抽牌)** | ⚠ 新写 | 见 C 节 |

### B2. 骨头人池新增效果(本次核实)

| 设计里的效果 | 可行性 | 实现方式(API + 出处) |
|---|---|---|
| 0费3点虚无 | ✅ | `CanonicalKeywords` 含 `CardKeyword.Ethereal` |
| 永久 -1 / -2 / -3 力量 | ✅ | `PowerCmd.Apply<StrengthPower>(..., -n, ...)`(同 B1) |
| **奥提斯死去** | ✅ | `CreatureCmd.Kill(player.Osty)`(`Player.Osty` 是 `Creature?`,`Player.IsOstyAlive`/`IsOstyMissing` 判存在)。**奥提斯已死 → 空打**(用户确认,损失能量不拿点) |
| **给所有手牌增加虚无** | ✅ | `foreach (var c in PileType.Hand.GetPile(player).Cards) c.AddKeyword(CardKeyword.Ethereal);`(`CardModel.AddKeyword(CardKeyword)` 存在,`CardModel.cs:1330`) |
| **召唤3** | ✅ 现成 | `OstyCmd.Summon(ctx, player, 3m, this)` —— **语义与设计完全一致**:奥提斯活着 → 加 x 生命上限;死了 → 复活并设 x 生命上限 + 满血(`OstyCmd.cs` 注释原文)。API 签名 `Summon(PlayerChoiceContext, Player summoner, decimal amount, AbstractModel? source)` |

**骨头人(Osty)机制关键点:** `OstyCmd.Summon` 已实现"复活/加上限"整套;「奥提斯死去」用 `CreatureCmd.Kill(player.Osty)`(`CreatureCmd.cs:445`,有 `force` 参数)。奥提斯替玩家挡伤害的 `DieForYouPower` 由原版 `Summon` 自动挂,不需处理。

### B3. 机器人池新增效果(本次核实,角色未做 → 暂缓)

| 设计里的效果 | 可行性 | 实现方式(API + 出处) |
|---|---|---|
| 向手牌加 1 张伤口 | ✅ 有先例 | `CombatState.CreateCard<Wound>(Owner)` + `CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, Owner)`(`Wound.cs` 是 Status 卡不可打出;加到 Hand 的先例:`Abundance.cs:40`) |
| 本回合 -2 敏捷 -2 力量 | ✅ | B1 的临时属性减,量改 2 |
| **触发一个充能球** | ✅ 已确认 | `OrbCmd.EvokeNext(ctx, player)`(**唤起 = 触发+移除**队首球,用户已确认;如将来想"只触发不移除"用 `OrbCmd.Passive`) |
| **这张牌打出两次** | ✅ 现成 hook | 壳覆写 `ModifyCardPlayCount`(原版 `DuplicationPower`/`BurstPower` 同款):`return card == this ? playCount + 1 : playCount;`。该 hook 在 `CardModel.GeneratePlayCount`(`CardModel.cs:2033`)调用,卡本身是 combat hook listener,天然生效 |
| 向抽牌堆加 1 张眩晕 | ✅ | Dazed + `AddGeneratedCardToCombat(card, PileType.Draw, Owner)`(同 B1,换 PileType.Draw) |
| 冲能球栏位减少1 | ✅ | `OrbCmd.RemoveSlots(player, 1)`(`OrbCmd.cs:41`,从队尾删) |
| -2力量 + 冲能球栏位增加1 | ✅ | `PowerCmd.Apply<StrengthPower>(-2)` + `OrbCmd.AddSlots(player, 1)`(`OrbCmd.cs:23`,上限 10) |
| 0费4点无效果 | ✅ | 空壳,`OnPlay` 空 |

**机器人关键点:** 充能球/栏位系统**原版完整存在**(`OrbQueue`、`OrbCmd`、Dark/Frost/Glass/Lightning/Plasma 各球、`NOrbManager` 节点、`CrackedCore/InfusedCore` 起始遗物)。但**机器人角色未做,本阶段暂缓**(用户确认),充能球类壳只有在角色有球栏位(`BaseOrbSlotCount>0`)时有意义。以上 API 留作将来做机器人时参考。

## C. 动作缓慢(Slowdown)实现设计(用户已定名/定规则)

**为什么不能复用游戏自带的 `SlowPower`:** 原版 SlowPower 语义完全不同(每打 1 牌 +1、所受攻击伤害 +10%、回合开始重置)。设计要的是「每层 -1 抽牌、永久到战斗结束」。**新建自定义 Power `SlowdownPower`**(显示名 eng "Slowdown" / zhs "动作缓慢")。

**参考模板(关键):** 原版 `MindRotPower.cs` 减回合开始抽牌:

```csharp
public override decimal ModifyHandDraw(Player player, decimal count)
    => player != base.Owner.Player ? count : Math.Max(0m, count - (decimal)base.Amount);
```

但 `Hook.ModifyHandDraw` **只在回合开始抽牌**时被调(`CombatManager.cs:877`,抽 5 之前)。设计要求**牌效抽牌也减**(「draw 3 有 2 层慢只能抽 1」),所以补一个钩子:

- 新建 `SlowdownPower : PowerModel`(`PowerType.Debuff` / `PowerStackType.Counter`),override `ModifyHandDraw` → 管回合开始抽牌。
- **Harmony Prefix** `CardPileCmd.Draw(PlayerChoiceContext ctx, decimal count, Player player, bool fromHandDraw)`:管牌效抽牌。`fromHandDraw==true`(回合开始那次)已在 `ModifyHandDraw` 消耗过 → **prefix 跳过,避免双扣**。这样两条抽牌路径全被减,且不碰 CombatManager 的 Innate 保底逻辑。
- **规则已确认(2026-08-11 改为消耗制)**:每次抽牌按抽牌数消耗层数。`consumed = min(层数, 抽牌数)`;实际抽数 = 抽牌数 − consumed;层数 -= consumed(**层数 < 抽数 → 抽数−层数、层数清 0;层数 ≥ 抽数 → 不抽牌、层数 −= 抽数**)。统一公式在 `SlowdownPower.Consume(decimal count)`(与 prefix 共用);`ModifyHandDraw` 调它管回合开始,prefix 调它管牌效。消耗到 0 时 `RemoveInternal()` 自我移除(不残留 0 层图标)。**不再永久、不再只减不回**。
- 层数显示/文案:本地化 `powers.PARTSMITH-SLOWDOWN_POWER.title/.description/.smartDescription`(eng+zhs 的 `powers.json`)。当前 eng: "Whenever you draw, spend Slowdown to draw fewer cards. Each card not drawn costs 1 Slowdown." / zhs: "每次抽牌时消耗动作缓慢层数少抽等量的牌;每少抽 1 张牌消耗 1 层动作缓慢。" 图标:`ICustomPower` + `CustomPackedIconPath`(指 BaseLib 的 `baselib-power_temp_down.png`)。

## D. 卡池组织(按用户方案:旧 14 移除,只保留设计池)

| 角色 | 池 | 组成 | 卡数 |
|---|---|---|---|
| 铁甲战士 | `PartSmithCostCardPool` | 共享17 + 战士7 | 24 |
| 小猎人 | `PartSmithHunterCostCardPool` | 共享17 + 猎人7 | 24 |
| 王(储君) | `PartSmithWangCostCardPool` | 共享17 + 王7 | 24 |
| 骨头人 | `PartSmithBoneManCostCardPool` | 共享17 + 骨头人7 | 24 |
| 机器人 | (未建池) | 暂缓不做 | — |

1. **旧 14 壳从池移除,类直接删除**(Scrap/Girder/Plank/ChainShell/RustShell/WhetShell/CrumblingShell/DeadweightShell/TitanShell/VoidShell/SocketedShell/BastionShell/BloodShell/IronShell + 各自瘦子类 + `CrumblingShell*DownPower` 两 Power)。连带:parttest/partsplice 默认宿主重指新共享#1(0费3点)。
2. **共享 17 张完整实现(不跳过)**:设计公共池 17 张全部新增。每张 = **战士池基类壳 + 猎人/王/骨头人瘦子类**(`XxxShellHunter/Wang/BoneMan : XxxShell` 只覆写 `[Pool]`,照现有瘦子类模式)。改效果只改基类一处 → **后续好改**(用户定)。共 17 基类 + 51 瘦子 = 68 类。
3. **角色专属 7 张**:战士 7 / 猎人 7 / 王 7 / 骨头人 7,只在该角色池(共 28 类)。
4. **本地化**:每张新卡都要 `eng/zhs cards.json` 的 `PARTSMITH-<蛇形类名>.title/.description`(无自身效果空壳 description = `""`);瘦子类也要各自 key;旧壳 loc key 一并清理。
5. **名字随便起**(用户确认):实现时按效果顺手起,不必逐张对齐。拼卡显示名 = 效果名逗号连接,壳名只在空壳(未拼)时出现,所以名字几乎不进玩家视线。

## E. 旧壳处置(用户已确认:池里不再含旧 14,类删除)

- 旧 14 壳 + 各自瘦子类**从池移除、类删除**(不是只摘 `[Pool]`,避免留死代码)。设计池的共享 17 已覆盖旧壳的核心效果(0费3点/0费6点稀有/1费6点/1费10点/2费15点 等点卡;本回合力量-1敏捷-1;力量-1;敌人加力量),旧壳独有效果(敌人+1力量精确值、自己-3血精确值、1费10点罕见等)随删除消失。
- **连带修改(必须做)**:①`PartSelfTestCommand.cs` / `SpliceTestCommand.cs` 默认宿主 `Scrap` → 改新共享#1(0费3点普通,规格与 Scrap 一致);②`CrumblingShellStrengthDownPower.cs` / `CrumblingShellDexterityDownPower.cs` 删除(`OriginModel` 指向已删的 CrumblingShell);③旧壳 eng/zhs loc key 清理。

## F. 实施阶段(建议顺序,每阶段保持可编译)

1. **Slowdown Power + Draw 钩子 + powers 本地化**(唯一全新机制,先做通、独立可测)。
2. **新增共享 17 基类**(战士池)+ 猎人/王/骨头人瘦子类(68 类)。
3. **删旧 14**:重指 parttest/partsplice 默认宿主 → 新共享#1 → 删 14 基类 + 42 瘦子类 + 2 Power + 旧 loc key。
4. **角色专属壳**:战士 7 / 猎人 7 / 王 7 / 骨头人 7(28 类)。
5. `eng/zhs cards.json` 补全部新 key + 清旧 key。
6. 机器人池**暂缓**(用户确认不做)。
7. 构建 0 错 0 警 → pck 重打(有本地化必须)→ `tools/deploy-to-game.sh` 部署 → 启动验证。

## G. 风险 / 注意点

- **奖励至少 3 张不同稀有度**(M2b 教训:池 <3 会崩)。每池 24 张、三稀有度都有,安全。
- **代码量**:17 共享 ×(1 基类 + 3 瘦子)= 68 + 角色专属 7×4 = 28 = **96 个新卡类**。瘦子类只覆写 `[Pool]`,逻辑集中在基类,后续改效果只动基类一处。
- **删旧连带**:parttest/partsplice 默认宿主已重指新共享#1;`CrumblingShell*DownPower` 已删;确认无其它代码引用旧壳类(grep 已查,仅 3 处)。
- **点数**:设计值很多是**手定平衡、偏离公式**(3费18点、2费6点、1费12点、3费30点、3费45点、1费15点…)。`PointCapacity` 按设计值写,「1费6点/2费10点/3费15点+稀有度加成」只是参考基准。
- **眩晕/伤口进牌堆**用 BoostAway 三行,`CombatState.CreateCard<X>(Owner)` 前确认 `Owner` 非空(拼卡打出时正常)。
- **多人**:所有效果走 `PowerCmd/CreatureCmd/CardPileCmd/OstyCmd/OrbCmd` 这些带 choiceContext 的命令天然同步;`SlowdownPower` 是纯数值 Power,`ModifyHandDraw`/Draw prefix 每机确定性计算,无选择同步问题(参照 [[multiplayer-desync-root-cause]] "全机确定性"原则)。

## H. 待确认项(2026-08-11 已全部确认)

1. 「触发一个充能球」= **唤起(触发+移除)** → `OrbCmd.EvokeNext(ctx, player)` ✅
2. 「奥提斯死去」奥提斯已死 → **空打**(损失能量不拿点) ✅
3. 机器人池 → **暂缓不做**(实现方式留作参考) ✅
4. 王池「-星辉」星辉为 0 → **照打可出**(`LoseStars` clamp 0,不报错) ✅
5. 新壳名字 → **随便起**(拼卡显示名=效果名,壳名仅空壳时出现) ✅

全部落实,可以按 §F 开工。

---
**✅ 已实现并部署(2026-08-11 22:20,构建 0 错 0 警,dll + pck 311884B 已部署到游戏 mods)**:
- §C Slowdown:新增 `SlowdownPower`(ModifyHandDraw 减回合抽牌)+ `DrawSlowdownPrefixPatch`(Harmony prefix 减牌效抽牌,fromHandDraw=true 跳过防双扣),powers.json eng/zhs(含 smartDescription)。
- §D 卡池:共享 17 基类(`SharedCostShells.cs`,战士池)+ 51 瘦子(`Hunter/Wang/BoneManSharedCostShells.cs`)+ 角色专属 28(4 文件各 7)= **96 类,每池正好 24 张**。
- §E 旧壳:14 基类 + 42 瘦子 + `CrumblingShell*DownPower` 已删;parttest/partsplice 默认宿主 → `TrinketShell`;旧 loc key 已清。
- 通用临时属性减 Power:`TempStrengthDownPower`/`TempDexterityDownPower`(OriginModel 指共享#5)。
- **待游戏内实测**:四个角色费用奖励各出 24 张池的卡;Slowdown 减抽牌(回合开始+卡片效果);奥提斯召唤/杀死;星辉增减;王/骨头人池图标配色。
- 手写壳类名避开缩写(HP/Str 开头)防 loc key 拆错。
