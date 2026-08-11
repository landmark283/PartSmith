# 王(储君/Regent fork)复刻方案

日期:2026-08-11 · 目标版本:StS2 v0.110.1 · 前置:BigWarrior + 小猎人(LittleHunter)拼接体系已跑通(M1/M2/M3/篝火钓鱼/图鉴)

## 目标

复刻储君(**Regent**,群星王座的继承人)为可玩角色「**王(Wang)**」,接入 PartSmith 拼卡体系:
费用卡 + 效果卡拼接;原版储君卡迁成效果卡,卡池/奖励走王专属池。
**核心卖点 = 原版双资源体系(能量 + 跨回合累积的辉星)免费继承**:
王用 `PlaceholderID="regent"`,辉星资源 / 铸造 / 君王之剑 / 仆从 Token / 星费双轨扣费 全是基游戏原生机制,直接可用。

用户已确认方向:复刻储君,名字叫「王」;沿用小猎人「新建专属池(不动大战士 ironclad 池)」的既定决策。
用户已确认决策(2026-08-11):**一次铺满 82 张效果卡**;**星费走「宿主携带星费」方案 A**。

---

## 阶段 A:原版 1:1 克隆(先做,可玩)

交付一个与原版储君**一模一样**的可玩副本,先验证角色 fork 本身。

- 新文件 `src/PartSmith/PartSmithCode/Characters/Wang.cs`:
  `Wang : PlaceholderCharacterModel`,`PlaceholderID = "regent"` →
  视觉/动画/音效/能量计数器/角色选择界面/辉星计数器全复用储君(BaseLib 官方捷径)。
- 数值完全照抄原版 Regent(反编译 `MegaCrit.Sts2.Core.Models.Characters.Regent.cs` 已核实):
  - `Gender = Masculine`,`NameColor = StsColors.orange`
  - `StartingHp = 75`,`StartingGold = 99`
  - **`ShouldAlwaysShowStarCounter => true`**(储君专有覆盖:始终显示辉星计数器)
  - `StartingDeck` = 4×StrikeRegent + 4×DefendRegent + FallingStar(0能2星攻击) + Venerate(1能获得2星)
  - `StartingRelics = [DivineRight]`(每场战斗进战斗房 +3 星 → 星体系引擎)
  - `CardPool = RegentCardPool`、`RelicPool = RegentRelicPool`、`PotionPool = RegentPotionPool`
  - `UnlocksAfterRunAs = ModelDb.Character<Silent>()`(保持原版解锁链位置)
- 本地化(ModAnalyzer STS001 必填,参考 BigWarrior/LittleHunter):
  - `localization/eng/characters.json`:`PARTSMITH-WANG.*`(title/description/代词/goldMonologue/aromaPrinciple/banter.alive|dead.endTurnPing 等)
  - `localization/eng/ancients.json`:`THE_ARCHITECT.talk.PARTSMITH-WANG.*`
  - `localization/zhs/characters.json` 同步简中
- **验证点**:角色选择界面出现「王」,进局玩法与储君一致(星费卡灰显/扣星、君王之剑能锻造出来、辉星计数在 UI 上显示)。

---

## 阶段 B:卡池与卡片奖励改造

### B1 王专属池(沿用小猎人决策:新建专属池)

`src/PartSmith/PartSmithCode/Pools/PartSmithCardPools.cs` 新增:

| 池 | 图标 | 用途 |
|---|---|---|
| `PartSmithWangCostCardPool` | regent 橙 | 王费用卡 |
| `PartSmithWangEffectCardPool` | regent 橙 | 王效果卡 |

- 图标资源(游戏 pck 已核实存在,`energy_regent` 3 处 / `regent_energy_icon` 15 处):
  - `BigEnergyIconPath` = `res://images/atlases/ui_atlas.sprites/card/energy_regent.tres`
  - `TextEnergyIconPath` = `res://images/packed/sprite_fonts/regent_energy_icon.png`
  - `EnergyOutlineColor` = `#803D0E`(原版 RegentCardPool 配色)
  - `DeckEntryCardColor` = `#E36600`
- **费用卡壳**:新建 14 个轻量子类注册进王池(`WangCostShells.cs`,`XxxWang : XxxShell` + `[Pool(typeof(PartSmithWangCostCardPool))]`),
  复用战士壳全部逻辑,图标走 regent 橙。大战士/猎人的池全部不动。
- 池全部 `IsShared = true` + `SeenByDefault = true`(图鉴显示完整卡面)。

### B2 储君卡迁移成效果卡(~82 张)

- 数据源 = 反编译 `RegentCardPool.cs`(91 张)+ 各卡定义文件;91 − 4 Basic(StrikeRegent/DefendRegent/FallingStar/Venerate) − 5 仅多人(Constellation/HammerTime/Largesse/Plot/Tutor)= **82 张**效果卡。
- 生成:仿小猎人 `generate_hunter_effect_cards.py`,写一版 `generate_wang_effect_cards.py`(池→`PartSmithWangEffectCardPool`,类名 `<原版卡>Fragment`,本地化 key `PARTSMITH-<卡>_FRAGMENT.*`,显示名不带 Fragment 后缀)。
- `PortraitSourceCard => ModelDb.Card<原版储君卡>()` → 卡图/拼图直接复用原版储君原画(橙框)。

#### ⭐ 核心设计:星费在拼卡上的实现(先冒烟,风险最高的点)

原版储君卡是**能量+辉星双轨费**(`CanonicalStarCost`),而拼卡的"宿主壳"只带能量费。要让带星费的效果卡在拼卡上正确工作:

- **推荐方案 A(宿主携带星费)**:效果卡继承 `EffectCardModelBase` 时覆写 `CanonicalStarCost`;拼接/拆拼时把宿主卡(可变实例)的星费设为附着效果星费之和。基游戏的卡牌打出流程会**免费接管一切**:能量+星任一不足 → 灰显不可点;打出 → 自动 `SpendStars`;`LastStarsSpent` 就位 → `Stardust` 的 `ResolveStarXValue()` 直接可用。实现入口候选:`CardModel.SetStarCostThisCombat(cost)`(整场持续)或反射写 `_baseStarCost`(永久,随战斗实例复制),拆拼时重算。
- **备选方案 B(效果自管扣星)**:`ExecuteEffect` 开头 `await PlayerCmd.LoseStars(starCost, player)`,不足则弹回。实现最简单,但**无灰显**(星不足时照样可点)→ 玩家体验差,仅作 A 受阻时的兜底。
- 星费卡清单(见下 ② 类辉星系 ~25 张)。

#### 储君独有机制逐项处理表(全部可用基游戏原生命令)

| 机制 | 迁移方式 | 涉及卡 |
|---|---|---|
| **辉星获取/消耗** | `PlayerCmd.GainStars/LoseStars`(角色无关,任何 player 可用) | Venerate, GatherLight, Glow, SolarStrike, HiddenCache, RoyalGamble, Genesis(能力)… |
| **星费扣费** | 方案 A(宿主携带星费) | FallingStar, Alignment, AstralPulse, CloakOfStars, Comet, Devastate, DyingStar, GammaBlast, GuidingStar, MeteorShower, NeutronAegis, ParticleWall, Resonance, SevenStars, Stardust(X)… |
| **铸造 / 君王之剑** | `ForgeCmd.Forge(amount, player, source)` — 首次铸造自动生成君王之剑入手的全部逻辑在原生(含 VFX/SFX,硬编码 regent_forge) | WroughtInWar, Bulwark, Conqueror, RefineBlade, SpoilsOfBattle, SummonForth, BigBang, TheSmith, BeatIntoShape, Furnace(能力自动铸), SeekingEdge |
| **君王之剑联动能力** | 原生 Power 类(`SeekingEdgePower`/`ParryPower`/`SwordSagePower`)直接 PowerCmd 施加 | Parry, SeekingEdge, SwordSage, SummonForth |
| **仆从(Token 转化)** | 用卡牌转化指令把手牌/抽牌堆卡换成原生 Token(`MinionStrike`/`MinionDiveBomb`/`MinionSacrifice`),自克隆类效果**必须克隆宿主拼卡**(小猎人 Anger/Outrage 已踩坑) | Begone, Charge, Guards |
| **能力牌(Hook 型)** | 原生 Power 类(GenesisPower 每回合+星 / FurnacePower 自动铸造 / ChildOfTheStarsPower 耗星+护甲 / TyrannyPower / VoidForm 等) | Genesis, Furnace, ChildOfTheStars, Tyranny, VoidForm, MonarchsGaze, Monologue, Orbit, SpectrumShift, Royalties… |
| **星X费(Stardust)** | 需宿主 `HasStarCostX` + `ResolveStarXValue()`;方案 A 下原生处理 | Stardust |
| **仅多人卡** | 跳过不迁移 | Constellation, HammerTime, Largesse, Plot, Tutor |
| **稀有度/Ancient/Epoch** | MeteorShower/TheSealedThrone 为 Ancient(通关解锁),保留原稀有度;Epoch 解锁过滤(Regent2/5/7Epoch)原样保留 | — |

#### 手写体(机制翻译需要逐卡核,同小猎人 B2 手写清单)

- **CrescentSpear**("每张有星费的卡 +2 伤害"):原版按战力池卡数算,拼卡体系下改按**手牌+牌组里带星费的卡数**或常值,方案评审时定。
- **Radiate**(伤害×本回合获得星数):需要"本回合得星计数",用 combat-scoped 计数器 + `AfterStarsGained` hook 或本地累计。
- **LunarBlast / MakeItSo / KinglyKick / KinglyPunch**(回合内技能数/永久成长/抽到减费):逐卡按原版 `CombatManager.History` 语义翻译。
- **BeatIntoShape**(按本回合已造伤铸造)、**VoidForm**(回合内打 2 张后全 0 费)、**IAmInvincible**(抽牌堆顶自动打)、**TheSealedThrone**(封印王座):状态/触发复杂,单列手写。

### B3 奖励与角色接入

- `Wang.CardPool` → 保持 `RegentCardPool`(商店/事件给原版储君卡;**不要**设成王费用池——商店按角色 CardPool 的卡类型+稀有度出卡,14 壳全 Skill 会导致商人要 Attack 卡时崩溃,小猎人已踩)。
- `Patches/RewardInjectionPatch.cs`:条件扩成 `is BigWarrior or LittleHunter or Wang`;王走王专属池(普通 1 费用+1 效果,精英 1+2)。
- `Rewards/PartSmithRewardFactory.cs`:新增 `CreateWangCostCardReward` / `CreateWangEffectCardReward`(从王池出卡,稀有度权重同猎人:普通 RegularEncounter / 精英 EliteEncounter)。
- `Patches/RestSiteOptionInjectionPatch.cs`:王也能钓鱼(拆拼/重组)。
- `Patches/PartSmithCardLibraryFiltersPatch.cs`:给两个王池补图鉴过滤按钮;王局内默认选王费用池。

---

## 交付顺序(每条都构建 0 错 0 警 → pck 重打包 → 部署 → 启动验证)

1. **A1 王克隆**:角色类 + 中英本地化 + pck 重打包部署。→ 角色可选、辉星计数显示、玩法与储君一致。
2. **B1 王专属池 + 壳子类**:两个池 + 14 壳子类。→ 王卡图标变 regent 橙。
3. **B2a 星费冒烟(最高风险前置)**:先做「效果卡覆写 CanonicalStarCost + 宿主携带星费」最小实现,用 `parttest make` 造一张星费卡,验证:灰显/扣星/Stardust ResolveStarXValue。确认方案 A 可行再铺全量。
4. **B2b 王效果卡**:生成脚本 + 82 张效果卡 + 本地化 + 手写体。→ 每张可拼、星费/铸造/君王剑机制正确。
5. **B3 奖励/篝火/图鉴接入**:注入 patch 扩展 + 王奖励工厂 + 图鉴按钮。

## 完成状态(实施中)

- ✅ **A1**(2026-08-11):`Characters/Wang.cs`(PlaceholderID="regent",纯副本:StartingHp=75/起始卡组 4×StrikeRegent+4×DefendRegent+FallingStar+Venerate/DivineRight/RegentCardPool/ShouldAlwaysShowStarCounter/EnergyLabelOutlineColor #784000FF)+ 本地化(eng/characters.json + ancients.json 加 `PARTSMITH-WANG.*`)。构建 0 错 0 警;dll/json/pdb 部署 + pck 重打包完成;**待游戏内实测**(角色可选/辉星计数/星费卡灰显/君王之剑)。
- ✅ **B1**(2026-08-11):`Pools/PartSmithCardPools.cs` 新增 `PartSmithWangCostCardPool`/`PartSmithWangEffectCardPool`(regent 橙:energy_regent.tres + regent_energy_icon.png + 描边 #803D0E + DeckEntryCardColor #E36600/#FF9E2C);`Cards/CostCards/WangCostShells.cs` 14 个轻量子类(`XxxWang : XxxShell`,`[Pool]` 归王池)+ 本地化(eng/zhs cards.json 各 14 组 `PARTSMITH-*_WANG.*`)。构建 0 错 0 警 + pck 重打包部署。待游戏内看王卡图标变 regent 橙。
- ✅ **B2a 星费冒烟(方案 A 宿主携带星费,2026-08-11 游戏内全 PASS)**:`EffectCardModelBase.StarCost`(virtual,0=无)+ `SpliceController.StarCostOf/RefreshHostStarCost`(反射写宿主 `_baseStarCost`+`_starCostSet`,AttachEffect/DetachAllEffects/parttest AttachUnchecked 三处同步)+ `CostCardModelBase.HasStarCostX`(从已拼效果传播,Stardust 用)。冒烟卡 `StarCostSmokeFragment`(星费2,仿 FallingStar)+ `StarXSmokeFragment`(HasStarCostX,仿 Stardust)注册进王效果池。**游戏内验证(静默角色战斗,桥接驱动)**:固定星费 星费=2 宿主携带 ✅ / 星0灰显不可打→星3可打 ✅ / 打出扣2星(3→1)✅ / 造成8伤 ✅;X星费 星0也可打(X语义)✅ / 耗光所有星(3→0)✅ / ResolveStarXValue=3 → 5×3=15伤 ✅。方案 A 可行,铺全量无风险。⚠ 两张冒烟卡仍在王效果池,后续可清理。
- ✅ **B2b 82 张王效果卡(2026-08-11,构建0错0警+pck部署,池注册+机制抽查通过)**:
  `build_wang_effect_cards.py`(82 卡清单=91−4 Basic−5 仅多人;从反编译源提取 ctor/vars/onPlay/星费 `CanonicalStarCost`/HasStarCostX;pointCost 0/1/2/3→3/6/10/15+稀有度)+ `generate_wang_effect_cards.py`(池→PartSmithWangEffectCardPool,类名 `<原版卡>Fragment`,id `PARTSMITH-<蛇形>_FRAGMENT`;发射 `StarCost`/`HasStarCostX`;本地化 eng+zhs;6 张 base.SelectionScreenPrompt 卡补 `selectionScreenPrompt` 键)。
  **手写体 10 张**:CalculatedVar 单例陷阱(CrescentSpear/Radiate/LunarBlast/BeatIntoShape/Supermassive/Stardust,手动从宿主上下文算 + 剥 Calculated* var 防预览 NRE + 描述去占位符);丢 hook 卡(MakeItSo/KinglyKick/KinglyPunch/IAmInvincible,原版 AfterCardDrawn/AfterCardPlayedLate/AfterAutoPostPlayPhaseEntered 在效果卡上无法触发,只移植 OnPlay,描述改只描述实际效果,KinglyKick 4费点值手定为 20)。
  **游戏内抽查(bridge 驱动,静默角色战斗)**:`parttest library wang_effect` = 84 卡全注册进王效果池 ✅;Alignment(星3)灰显→星5可打→扣3得2能量 ✅;TheSmith(星4)扣4→锻造→君王之剑 40 伤(基础10+锻30)✅;Stardust(X星)星0也可打(X语义)✅;王池 regent 能量图标渲染 ✅。**每张卡的完整战斗验证留用户实测**。
- ✅ **B3 奖励/篝火/图鉴接入(2026-08-11,构建0错0警+pck部署)**:`RewardInjectionPatch` 条件扩 `BigWarrior or LittleHunter or Wang` + CostReward/EffectReward 3 路分派;`PartSmithRewardFactory` 加 `CreateWangCostCardReward`/`CreateWangEffectCardReward`;`RestSiteOptionInjectionPatch` 王可钓鱼;`PartSmithCardLibraryFiltersPatch` 加王池两按钮(E36600/#FF9E2C)+ 王局内默认选王费用池;card_library.json eng/zhs 补王 tip。**待游戏内验证:开一局王,看奖励=费用+效果双槽、篝火钓鱼、图鉴王池按钮**。
- ✅ **用户实测(2026-08-11)**:开了一局「王」,确认没有大问题。
- ✅ **冒烟卡清理(2026-08-11)**:`StarCostSmokeFragment`/`StarXSmokeFragment` 的 .cs + .uid + 本地化已删;王效果池回到 **82**、费用池 14。星费机制基础设施(`EffectCardModelBase.StarCost` / `SpliceController.RefreshHostStarCost` / `CostCardModelBase.HasStarCostX`)保留,是正式卡在用。
- ✅ **桥接复验**:角色选择 index 7 = 王;王进战斗 DivineRight 开局 +3 星。

## 风险 / 注意

- **星费是最大风险**:方案 A 的宿主携带星费若在拼卡上不生效(基游戏扣费只看宿主卡),备选 B 兜底(自管扣星,牺牲灰显)。**先冒烟再铺量**。
- **起始卡组是原版储君卡**(FallingStar/Venerate 教星费)——星体系在 A1 就应可用;若起始星费卡灰显逻辑对原版卡没问题(原生),则问题只在拼卡合成卡上。
- 星费效果卡池 3 选 1 需要 ≥3 卡(82 张远多,无风险)。
- 储君卡的稀有度/费分布与战士不同,pointCost 规则沿用战士的(0/1/2/3 → 3/6/10/15 + 稀有度 0/1/2/3)。
- 效果卡升级仍走 `GetUpgradeDelta`/`UpgradeEnergyGain`(禁调效果卡自身 OnUpgrade)。
- 角色本地化 key = `PARTSMITH-` + 类名大写下划线(`PARTSMITH-WANG`)。
- 王用 `PlaceholderID="regent"`,**不应再覆写 GenerateAnimator 等**(否则丢储君动画);数值类(HP/金币/卡池)照抄即可。
