# 机器人(故障机器人/Defect fork)复刻方案

日期:2026-08-12 · 目标版本:StS2 v0.110.1 · 前置:BigWarrior + 小猎人 + 王 + 骨头人拼接体系已跑通(M1/M2/M3/篝火钓鱼/图鉴)

## 目标

复刻故障机器人(**Defect**)为可玩角色「**机器人(Robot)**」,接入 PartSmith 拼卡体系:
费用卡 + 效果卡拼接;原版故障机器人卡迁成效果卡,卡池/奖励走机器人专属池。
**核心卖点 = 原版充能球体系(球槽/充能/唤起/被动/Dark/Frost/Glass/Lightning/Plasma)免费继承**:
机器人用 `PlaceholderID="defect"` + `BaseOrbSlotCount=3`,充能球/栏位/唤起全是基游戏原生机制。

用户已确认方向:复刻故障机器人,名字叫「机器人」;沿用小猎人「新建专属池(不动大战士 ironclad 池)」的既定决策。

---

## 阶段 A:原版 1:1 克隆(先做,可玩)

- 新文件 `src/PartSmith/PartSmithCode/Characters/Robot.cs`:
  `Robot : PlaceholderCharacterModel`,`PlaceholderID = "defect"` →
  视觉/动画/音效/能量计数器/角色选择界面/充能球栏位全复用故障机器人(BaseLib 官方捷径)。
- 数值完全照抄原版 Defect(反编译 `MegaCrit.Sts2.Core.Models.Characters.Defect.cs` 已核实):
  - `Gender = Neutral`(中性),`NameColor = StsColors.blue`
  - `StartingHp = 75`,`StartingGold = 99`
  - **`BaseOrbSlotCount = 3`**(充能球引擎关键:Player 构造按它初始化球槽容量,PlayerCombatState.cs:139 `OrbQueue.AddCapacity(player.BaseOrbSlotCount)`,OrbCmd.Channel 也用它判自动加槽)
  - `StartingDeck` = 4×StrikeDefect + 4×DefendDefect + Zap + Dualcast
  - `StartingRelics = [CrackedCore]`(战斗开始自动充 1 闪电球)
  - `CardPool = DefectCardPool`、`RelicPool = DefectRelicPool`、`PotionPool = DefectPotionPool`
  - 动画/配色:AttackAnimDelay 0.15 / CastAnimDelay 0.25 / PowerUpAnimDelay 0.5 / EnergyLabelOutlineColor #163E64FF /
    DialogueColor #13446B / SpeechBubbleColor Blue / MapDrawingColor #0D638C / RemoteTargetingLineColor #70B6EDFF /
    RemoteTargetingLineOutline #163E64FF / CharacterTransitionSfx wipe_ironclad / GetArchitectAttackVfx 5 个(闪电/钝击/抓挠/挥打/重击)
- 本地化(ModAnalyzer STS001 必填,参考其他角色):
  - `localization/eng/characters.json`:`PARTSMITH-ROBOT.*`(title/description/代词/goldMonologue/aromaPrinciple/banter.alive|dead.endTurnPing 等)
  - `localization/eng/ancients.json`:`THE_ARCHITECT.talk.PARTSMITH-ROBOT.*`
- **验证点**:角色选择界面出现「机器人」,进局玩法与故障机器人一致(充能球栏位 3 格、开局闪电球、卡池/奖励/遗物/药水全原版)。

## 阶段 B:卡池与卡片奖励改造

### B1 机器人专属池 + 壳子类

`src/PartSmith/PartSmithCode/Pools/PartSmithCardPools.cs` 新增:

| 池 | 图标 | 用途 |
|---|---|---|
| `PartSmithRobotCostCardPool` | defect 蓝 | 机器人费用卡 |
| `PartSmithRobotEffectCardPool` | defect 蓝 | 机器人效果卡 |

- 图标资源(游戏 pck 已核实存在,`energy_defect` 3 处 / `defect_energy_icon` 4 处):
  - `BigEnergyIconPath` = `res://images/atlases/ui_atlas.sprites/card/energy_defect.tres`
  - `TextEnergyIconPath` = `res://images/packed/sprite_fonts/defect_energy_icon.png`
  - `EnergyOutlineColor` = `#1D5673`(原版 DefectCardPool 配色),`DeckEntryCardColor` 费用 #3EB3ED / 效果 #5AC8FA
- **费用卡壳**(`Cards/CostCards/RobotSharedCostShells.cs` + `RobotCharacterCostShells.cs`):
  - 17 个共享壳瘦子类(`XxxShellRobot : XxxShell`,`[Pool]` 归机器人池)——共享牌每角色瘦子类保配色
  - 8 个机器人专属壳(设计见 `费用kards.md §机器人池`):伤口入手(0费6点罕见)/临时削弱(0费6点罕见,-2temp str -2temp dex)/
    唤起充能球(0费3点罕见,`OrbCmd.EvokeNext`)/双重打出(2费12点稀有,`ModifyCardPlayCount` 同 DuplicationPower)/
    眩晕入抽牌堆(1费10点普通)/充能球栏位减少(1费15点稀有,`OrbCmd.RemoveSlots`)/充能球栏位增加(3费30点普通,-2力量+`OrbCmd.AddSlots`)/芯片(0费4点罕见,空壳)
- 池全部 `IsShared = true` + `SeenByDefault = true`。

### B2 机器人卡迁移成效果卡(87 张)

- 91 张原版 Defect 池卡 − 4 Basic(StrikeDefect/DefendDefect/Zap/Dualcast)= **87 张**效果卡。
- 生成:`build_robot_effect_cards.py`(提取 ctor/vars/onPlay/升级,数据源=反编译源+vanilla_loc)
  + `generate_robot_effect_cards.py`(池→`PartSmithRobotEffectCardPool`,类名`<原版卡>Fragment`,id `PARTSMITH-<蛇形>_FRAGMENT`,本地化 eng+zhs)。
- `PortraitSourceCard => ModelDb.Card<原版故障机器人卡>()` → 卡图/拼图直接复用原版原画(蓝框)。

#### 机器人独有机制逐项处理表(全部可用基游戏原生命令)

| 机制 | 迁移方式 | 涉及卡 |
|---|---|---|
| **充能球充能** | `OrbCmd.Channel<LightningOrb/FrostOrb/DarkOrb/PlasmaOrb/GlassOrb>`(角色无关,任何 player 可用) | BallLightning, ColdSnap, Darkness, Fusion, Glacier, Glasswork, Rainbow… |
| **唤起球** | `OrbCmd.EvokeNext/EvokeLast` | Dualcast(Basic 排除), MultiCast, Quadcast, Shatter, 机器人壳唤起充能球 |
| **球被动触发** | `OrbCmd.Passive` | Darkness, TeslaCoil, 壳 |
| **球槽增/减** | `OrbCmd.AddSlots/RemoveSlots` | Capacitor, Modded, BulkUp, 机器人壳 |
| **聚合(Focus)** | `PowerCmd.Apply<FocusPower>` | Defragment, BiasedCognition, Hotfix, Hyperbeam(-聚合), FocusedStrike, Synchronize |
| **充能球 Power(回合钩子)** | 原生 Power 类直接 PowerCmd | Loop, Storm, Thunder, Buffer, Hailstorm, Smokestack, Coolant, Subroutine, Feral, Iteration… |
| **X 费** | 手写:`X = 当前能量` → `PlayerCmd.LoseEnergy(X)` → 充能/唤起 X 次(升级 +1) | MultiCast, Tempest |
| **CalculatedVar 单例陷阱** | 手写从宿主上下文算 + 剥 Calculated* var + 描述去 InCombat 行 | Barrage, CompileDriver, FlakCannon, HelixDrill, Synchronize, Voltaic |
| **自克隆** | 克隆宿主拼卡(`cardPlay.Card.CreateClone()`),不克隆效果卡单例(同小猎人 Anger 已踩坑) | AdaptiveStrike |
| **成长(跨卡/永久)** | `ClawExtraModifier`(所有爪击拼卡共享成长)+ `GenAlgoExtraModifier`(宿主+牌组版本双更新) | Claw, GeneticAlgorithm |
| **选牌界面** | Hologram 用 `base.SelectionScreenPrompt` → 补 `selectionScreenPrompt` 本地化键(Headbutt 已踩坑) | Hologram |
| **私人字段内联** | CanDrawCard 是 private → 手动内联(本回合已打牌数 < PlayMax) | Ftl |
| **System 类型名冲突** | Buffer/Void 全限定 `global::MegaCrit.Sts2.Core.Models.Cards.X`(CS0104) | Buffer, Turbo |

#### 手写体清单(SPECIAL,机制翻译逐卡核)

- **Barrage/CompileDriver/FlakCannon/HelixDrill/Voltaic**(CalculatedVar):手动从宿主上下文算命中/抽牌数
  (球数/球类型数/状态牌数/本回合耗能/本回合充的闪电球数);剥 Calculation*/Calculated* var;描述去 `{InCombat:\n(...)|}` 行。
- **Synchronize**(CalculatedVar 但主文本用 `{CalculationExtra:diff()}`):保留 CalculationExtraVar 做预览,
  只剥 CalculatedFocus/CalculationBase;focus = 球类型数 × (2 + 升级级数)。
- **MultiCast/Tempest**(X 费):`X = 当前能量`,`PlayerCmd.LoseEnergy(X)` 全花,升级 +1;唤起/充能 X 次。
- **AdaptiveStrike**(自克隆):克隆宿主拼卡 `CreateClone()` + `SetThisCombat(0)` 入弃牌堆。
- **Claw**(成长):所有带爪击效果的拼卡共享成长(`ClawExtraModifier`,遍历 `PlayerCombatState.AllCards` 找 EffectCardId==本卡)。
- **GeneticAlgorithm**(永久成长):`GenAlgoExtraModifier` 存宿主战斗实例 + 宿主牌组版本(`DeckVersion`)双更新,实现"本局永久"。
- **Ftl**:`CanDrawCard` private 内联。
- **MeteorStrike**:5 费稀有,pointCost 手定 **25**(规则只到 3 费)。
- **AllForOne**:原版 `Filter` private 方法内联(0 费、非 X 费、类型=攻击/技能/能力)。
- **Hologram**:补 `PARTSMITH-HOLOGRAM_FRAGMENT.selectionScreenPrompt` 键(eng "Choose a card to put into your Hand." / zhs "选择一张牌放入你的手牌。")。
- **MachineLearning**:升级加 Innate → `UpgradeKeyword => CardKeyword.Innate`(EffectAttachmentModifier.OnUpgrade 补到宿主)。
- ⚠ **升级去除关键词不迁移**(Chill 去 Exhaust/EchoForm 去 Ethereal/Fusion/Hologram/Hotfix/Ignition/Rainbow/Voltaic 去 Exhaust):
  拼卡体系只支持升级加关键词(`UpgradeKeyword`),不支持去;升级后仍带该关键词。与猎人 DemonicShield 去 Exhaust 未迁移同款已知偏差。

### B3 奖励/篝火/图鉴接入

- `Robot.CardPool` → 保持原版 `DefectCardPool`(商店/事件给原版故障机器人卡;**不要**设成机器人费用池——商店按角色 CardPool 出卡,14 壳全 Skill 会触发猎人踩过的 merchant 崩溃)。
- `Patches/RewardInjectionPatch.cs`:条件扩 `is BigWarrior or LittleHunter or Wang or BoneMan or Robot`;机器人走机器人专属池(普通 1 费用+1 效果,精英 1+2)。
- `Rewards/PartSmithRewardFactory.cs`:新增 `CreateRobotCostCardReward` / `CreateRobotEffectCardReward`(从机器人池出卡,稀有度权重同其他角色)。
- `Patches/RestSiteOptionInjectionPatch.cs`:机器人也能钓鱼。
- `Patches/PartSmithCardLibraryFiltersPatch.cs`:加机器人池两按钮(费用 #3EB3ED / 效果 #5AC8FA);机器人局内默认选机器人费用池。
- `card_library.json` eng/zhs 补机器人 tip。

---

## 交付顺序(每条都构建 0 错 0 警 → pck 重打包 → 部署 → 启动验证)

1. **A1 机器人克隆**:角色类 + 中英本地化 + pck 重打包部署。→ 角色可选、充能球栏位 3 格、玩法与故障机器人一致。
2. **B1 机器人专属池 + 壳子类**:两个池 + 17 共享瘦子 + 8 专属壳。→ 机器人卡图标变 defect 蓝。
3. **B2 机器人效果卡**:生成脚本 + 87 张效果卡 + 本地化 + 手写体。→ 每张可拼、充能球机制正确。
4. **B3 奖励/篝火/图鉴接入**:注入 patch 扩展 + 机器人奖励工厂 + 图鉴按钮。

## 完成状态(2026-08-12 全部交付并部署,构建 0 错 0 警,待游戏内实测)

- ✅ **A1**:`Characters/Robot.cs`(PlaceholderID="defect",BaseOrbSlotCount=3,纯副本)+ `Robot.cs.uid` + 本地化(PARTSMITH-ROBOT.* + ancients)。
- ✅ **B1**:`PartSmithRobotCostCardPool`/`PartSmithRobotEffectCardPool`(defect 蓝 energy_defect.tres + defect_energy_icon.png + 描边 #1D5673);
  `RobotSharedCostShells.cs` 17 瘦子 + `RobotCharacterCostShells.cs` 8 专属;cards.json eng+zhs 补 17×2 共享 + 8×2 专属 key(共享瘦子 loc 从基类复制,专属壳中文名手写)。
- ✅ **B2**:`partsmith_robot_effect_cards.json`(87 卡)+ `build_robot_effect_cards.py`/`generate_robot_effect_cards.py` + 87 张效果卡 .cs(+87 .uid)+ 本地化(含 Hologram selectionScreenPrompt);`ClawExtraModifier`/`GenAlgoExtraModifier` 两个成长暂存器。构建 0 错 0 警。
- ✅ **B3**:RewardInjectionPatch 5 角色分派;PartSmithRewardFactory 加 CreateRobotCostCardReward/CreateRobotEffectCardReward;RestSiteOptionInjectionPatch 机器人可钓鱼;PartSmithCardLibraryFiltersPatch 加机器人池两按钮 + 默认选中;card_library.json 补机器人 tip。
- ✅ **部署(2026-08-12)**:dll/json/pdb + pck 重打包(373776B,含全部新 key)已部署到 `D:/Steam/steamapps/common/Slay the Spire 2/mods/PartSmith/`。**待游戏内实测**。

## 风险 / 注意

- **充能球是最大风险**:机器人开局 3 球槽(CrackedCore 自动充 1 闪电球),充能球体系全原版——若球槽没出现,先查 `BaseOrbSlotCount` 是否生效(Player 构造/PlayerCombatState 都从它初始化)。
- **X 费**:MultiCast/Tempest 走共享 `XCostHelper.ResolveAndSpend`(快照当前能量→全花→返回 X,升级 +1),与原版 StS2 的 X 费语义一致。2026-08-12 全 5 角色 X 费卡统一:Skewer/Malaise/HeavenlyDrill 修崩、Dirge/Eradicate 恢复含 X、7 张点数一律 1。拼卡没有 X 费灰显(宿主壳非 X 费,能量 0 也能打出→ X=0 空打)。
- 机器人效果卡池 3 选 1 需要 ≥3 卡(87 张远多,无风险)。
- 原版故障机器人卡稀有度/费分布与其他角色不同,pointCost 规则沿用战士的(0/1/2/3 → 3/6/10/15 + 稀有度 0/1/2/3);MeteorStrike(5 费)手定 25。
- 效果卡升级仍走 `GetUpgradeDelta`/`UpgradeEnergyGain`/`UpgradeKeyword`(禁调效果卡自身 OnUpgrade)。
- 角色本地化 key = `PARTSMITH-` + 类名大写下划线(`PARTSMITH-ROBOT`)。
- ⚠ **升级去除关键词未迁移**(Chill/EchoForm/Fusion/Hologram/Hotfix/Ignition/Rainbow/Voltaic 升级去 Exhaust/Ethereal):升级后仍带该关键词,与猎人 DemonicShield 一致。
