# 小猎人(猎人/静默猎手 fork)复刻方案

日期:2026-08-09 · 目标版本:StS2 v0.110.1 · 前置:BigWarrior 拼接体系已跑通(M1/M2/篝火钓鱼)

## 目标

复刻猎人(静默猎手 **Silent**)为可玩角色「小猎人(Little Hunter)」,接入 PartSmith 拼卡体系:
费用卡 + 效果卡拼接;原版猎人卡迁成效果卡,卡池/奖励走猎人专属池。

用户已确认的关键决策:**新建猎人专属池**(不动大战士的 ironclad 图标池)。

## 阶段 A:原版 1:1 克隆(先做,可玩)

交付一个与原版猎人**一模一样**的可玩副本,先验证角色 fork 本身。

- 新文件 `src/PartSmith/PartSmithCode/Characters/LittleHunter.cs`:
  `LittleHunter : PlaceholderCharacterModel`,`PlaceholderID = "silent"` →
  视觉/动画/音效/能量计数器/角色选择界面全复用猎人(BaseLib 官方捷径)。
- 数值完全照抄原版 Silent(反编译 `MegaCrit.Sts2.Core.Models.Characters.Silent.cs` 已核实):
  - `Gender = Feminine`,`NameColor = StsColors.green`
  - `StartingHp = 70`,`StartingGold = 99`
  - `StartingDeck` = 5×StrikeSilent + 5×DefendSilent + Neutralize + Survivor
  - `StartingRelics = [RingOfTheSnake]`
  - `CardPool = SilentCardPool`、`RelicPool = SilentRelicPool`、`PotionPool = SilentPotionPool`
- 本地化(ModAnalyzer STS001 必填,参考 BigWarrior):
  - `localization/eng/characters.json`:`PARTSMITH-LITTLE_HUNTER.*`(title/description/代词/goldMonologue/aromaPrinciple/banter.alive|dead.endTurnPing 等)
  - `localization/eng/ancients.json`:`THE_ARCHITECT.talk.PARTSMITH-LITTLE_HUNTER.*`
  - `localization/zhs/characters.json` 同步简中
- **验证点**:角色选择界面出现「小猎人」,进局玩法与猎人完全一致(卡池/奖励/遗物/药水全原版)。

## 阶段 B:卡池与卡片奖励改造

### B1 猎人专属池(用户已定)

`src/PartSmith/PartSmithCode/Pools/PartSmithCardPools.cs` 新增:

| 池 | 图标 | 用途 |
|---|---|---|
| `PartSmithHunterCostCardPool` | silent 绿 | 小猎人费用卡 |
| `PartSmithHunterEffectCardPool` | silent 绿 | 小猎人效果卡 |

- 图标资源(游戏 pck 已核实存在):
  - `BigEnergyIconPath` = `res://images/atlases/ui_atlas.sprites/card/energy_silent.tres`
  - `TextEnergyIconPath` = `res://images/packed/sprite_fonts/silent_energy_icon.png`
  - `EnergyOutlineColor` = `#1A6625`(原版 Silent 池配色)
- **费用卡壳**:新建 14 个轻量子类注册进猎人池(如 `EmptyShellHunter : EmptyShell` + `[Pool(PartSmithHunterCostCardPool)]`),
  复用大战士壳的逻辑/卡面,图标走猎人池。大战士的 `PartSmithCostCardPool`/`PartSmithEffectCardPool` 保持 ironclad 不动。
- 池全部 `IsShared = true`(否则不进 AllCardPools)+ `SeenByDefault = true`(图鉴显示完整卡面)。

### B2 猎人卡迁移成效果卡

- 从反编译源生成「猎人效果卡」(类名 `<原版卡>Fragment`):
  91 张原版猎人卡 → 效果卡。排除规则同战士 84 张那次:
  4 张 Basic(StrikeSilent/DefendSilent/Neutralize/Survivor) + X费/超高费 + 不适用卡。
- 数据源 = `.tools/decompiled/sts2/MegaCrit.Sts2.Core.Models.Cards.*.cs`;
  复用 `generate_effect_cards.py` 生成脚本(改池指向 `PartSmithHunterEffectCardPool`,类名/本地化 key 同步)。
- `PortraitSourceCard => ModelDb.Card<原版猎人卡>()` → 卡图/拼图直接复用原版猎人原画(绿框)。
- 本地化:`cards.json` 加 `PARTSMITH-<猎人卡>_FRAGMENT.*`;显示名不带 Fragment 后缀(同战士)。
- **⚠ 猎人专属机制**(毒 Poison/Shiv/幻象等)onPlayCode 需逐卡按原版翻译,不能机械照搬。
- **⚠ 共享单例坑**(Anger/Outrage 已修):所有「自克隆/对队友克隆」类效果必须克隆**宿主拼卡**(`cardPlay.Card`),不能克隆效果卡单例。

### B3 奖励与角色接入

- `LittleHunter.CardPool` → `PartSmithHunterCostCardPool`。
- `Patches/RewardInjectionPatch.cs`:条件扩成 `is BigWarrior or LittleHunter`;小猎人走猎人专属池(普通 1 费用+1 效果,精英 1+2)。
- `Rewards/PartSmithRewardFactory.cs`:新增 `CreateHunterCostCardReward` / `CreateHunterEffectCardReward`(从猎人池出卡)。
- `Patches/RestSiteOptionInjectionPatch.cs`:小猎人也能钓鱼(拆拼/重组)。
- `Patches/PartSmithCardLibraryFiltersPatch.cs`:给两个猎人池补图鉴过滤按钮;小猎人局内默认选费用卡池。

## 交付顺序(每条都构建 0 错 0 警 → pck 重打包 → 部署 → 启动验证)

1. **A1 小猎人克隆**:角色类 + 中英本地化 + pck 重打包部署。→ 角色可选、玩法与猎人一致。
2. **B1 猎人专属池 + 壳子类**:两个池 + 14 壳子类。→ 小猎人卡图标变 silent 绿。
3. **B2 猎人效果卡**:生成脚本 + 猎人效果卡 + 本地化。
4. **B3 奖励/篝火/图鉴接入**:注入 patch 扩展 + 猎人奖励工厂。

## 完成状态(2026-08-10 全部交付并部署,游戏已启动待实测)

- ✅ **A1**:`Characters/LittleHunter.cs`(PlaceholderID="silent",纯副本)+ 本地化。
- ✅ **B1**:`Pools/PartSmithCardPools.cs` 新增 `PartSmithHunterCostCardPool`/`PartSmithHunterEffectCardPool`
  (silent 绿图标 energy_silent.tres + silent_energy_icon.png + 描边 #1A6625);
  `Cards/CostCards/HunterCostShells.cs` 14 个轻量子类(`XxxHunter : XxxShell`,`[Pool]` 归猎人池,
  利用 `[Pool] Inherited=true+AllowMultiple=false` 派生声明覆写继承)。大战士 ironclad 池未动。
- ✅ **B2**:87 张猎人效果卡(91 − 4 Basic),数据源 `.tools/decompiled/sts2/MegaCrit.Sts2.Core.Models.Cards.*.cs`
  + `vanilla_loc`(原版中英文描述),生成脚本 `build_hunter_effect_cards.py`(提取)+
  `generate_hunter_effect_cards.py`(生成,池→`PartSmithHunterEffectCardPool`)。
  手写体(GrandFinale 抽牌堆门槛 / UpMySleeve AddThisCombat / BouncingFlask 内联色 / Haze 内联 VFX /
  Finisher/Flechettes/Mirage 手动算值绕开 CalculatedVar 单例陷阱)。4 张 CalculatedVar 卡描述剥离
  `{InCombat:...|}` 计数行(效果单例 CombatState 恒 null → 显示 0)。
- ✅ **B3**:`RewardInjectionPatch` 条件扩为 `BigWarrior or LittleHunter` 并按角色分派池;
  `PartSmithRewardFactory` 加 `CreateHunterCostCardReward`/`CreateHunterEffectCardReward`;
  `RestSiteOptionInjectionPatch` 小猎人也能钓鱼;`PartSmithCardLibraryFiltersPatch` 加两个猎人池过滤按钮 +
  小猎人默认选猎人费用池;card_library.json 补猎人 tip。
- ⚠ **B3 商店闪退修复(2026-08-10)**:曾把 `LittleHunter.CardPool` 设为 `PartSmithHunterCostCardPool`,
  但商店商人按角色 CardPool 的卡牌类型+稀有度生成出售卡,猎人池 14 壳全 Skill、无 Attack/Power →
  商人要 Attack 卡时抛 `InvalidOperationException: Can't generate valid rarity for merchant card type Attack` 崩溃。
  修复:CardPool 回原版 `SilentCardPool`(与大战士 IroncladCardPool 同理)——商店/事件给原版猎人卡,
  战斗奖励仍由注入接管猎人费用/效果池,拼卡体系不受影响。

## 风险 / 注意

- 猎人效果卡池 3 选 1 需要 ≥3 卡(远多于 3,无风险)。
- 原版猎人卡稀有度/费分布与战士不同,pointCost 规则沿用战士的(0/1/2/3 → 3/6/10/15 + 稀有度 0/1/2/3)。
- 效果卡升级仍走 `GetUpgradeDelta`/`UpgradeEnergyGain`(禁调效果卡自身 OnUpgrade)。
- 角色本地化 key = `PARTSMITH-` + 类名大写下划线(`PARTSMITH-LITTLE_HUNTER`)。
