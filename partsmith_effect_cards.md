# PartSmith 效果卡牌数据说明

> 配套文件:`partsmith_effect_cards.json`(84 张战士效果卡的完整数据,供另一个 AI 导入模组)。
> 机制背景:PartSmith 把"战斗后获得一张卡"改成"获得费用卡 + 效果卡 → 当场拼接成新卡"。本文件是**效果卡**的数据源(效果照抄原版铁甲战士卡)。

---

## 1. 数据是什么

`partsmith_effect_cards.json` 包含 **84 张效果卡**,效果与原版 StS2 铁甲战士卡 100% 一致(数值、逻辑、关键词),另带拼接机制所需的**点数消耗**。

- **数据源**:反编译 `sts2.dll` 的 `IroncladCardPool`(共 90 张,排除 6 张后为 84)。
- **每张卡**都附有原版反编译代码(`onPlayCode` / `onUpgradeCode`),作为效果实现的最权威参照。

## 2. 生成规则

### 2.1 选取范围

| 处理 | 卡 | 原因 |
|---|---|---|
| 保留 | 其余 84 张 | 战士常规可选卡 |
| 排除 | `StrikeIronclad` / `DefendIronclad` / `Bash`(Basic) | 基础卡不进效果池(用户定) |
| 排除 | `Whirlwind` / `Cascade`(X 费用)、`Midnight`(12 费) | X 费/超高费超出费用映射范围(用户定) |
| 保留但特殊 | `Break` / `Corruption`(Ancient) | 保留,稀有度加成按 +3(用户定) |

### 2.2 点数消耗(用户定规则)

```
pointCost = fee(originalCost) + rarityBonus(rarity)

fee:      0费→3   1费→6   2费→10   3费→15
rarity:   Common→0   Uncommon→1   Rare→2   Ancient→3
```

结果范围:3 ～ 18(分布见 §5)。

### 2.3 稀有度保留

每张卡的 `rarity` 与原版一致(Common 20 / Uncommon 37 / Rare 25 / Ancient 2),用于**奖励出现概率**(后续按稀有度调权重)。

## 3. 每张卡字段

| 字段 | 含义 | 示例(UppercutFragment) |
|---|---|---|
| `id` | 游戏内本地化 key = `PARTSMITH-` + 类名 SNAKE_CASE | `PARTSMITH-UPPERCUT_FRAGMENT` |
| `name` | 原版类名 | `Uppercut` |
| `className` | 建议 C# 类名(原版名 + `Fragment`) | `UppercutFragment` |
| `title` | 本地化显示名(直接取原版名,无 " Fragment" 后缀) | `Uppercut` |
| `titleZh` | **简体中文**卡名(直接取原版中文名,无 " 碎片" 后缀) | `上勾拳` |
| `portraitFrom` | 复用的**原版卡类型名**(效果卡卡图来源,见 §4 卡图) | `Uppercut` |
| `description` | 英文效果描述(攻击/技能准确;Power 为通用描述) | `Deal 13 damage to an enemy. Apply 1 Weak. Apply 1 Vulnerable.` |
| `descriptionZh` | **简体中文**效果描述(与效果一致) | `对一名敌人造成 13 点伤害。施加 1 层虚弱。施加 1 层易伤。` |
| `note` | 补充说明(升级数值、Power 行为、特殊逻辑) | `Upgrade: weak/vulnerable +1.` |
| `originalCost` | 原版能量费用 | `2` |
| `pointCost` | 拼接点数消耗(= `fee + rarityBonus`) | `11` |
| `type` | `Attack` / `Skill` / `Power` | `Attack` |
| `rarity` | 原版稀有度 | `Uncommon` |
| `target` | 目标类型(`Self`/`AnyEnemy`/`AllEnemies`/`AnyAlly`/`RandomEnemy`) | `AnyEnemy` |
| `keywords` | 卡牌关键词(`Exhaust`/`Ethereal`/`Innate`/`Retain`/`Unplayable`…) | `[]` |
| `tags` | 行为标签(`Strike`/`Defend`…) | `[]` |
| `gainsBlock` | 是否提供格挡(某些遗物/机制依赖) | `null` |
| `dynamicVars` | 动态数值(伤害/格挡/层数等,原版取值) | `[{"type":"DamageVar","args":["13m",...]},...]` |
| `onPlayCode` | **原版反编译 `OnPlay` 方法体**(效果实现权威) | — |
| `onUpgradeCode` | 原版反编译 `OnUpgrade` 方法体(行为性升级卡为注释占位,见 §9) | — |
| `upgrade` | **结构化升级数据**(见 §9) | 见 §9 |

## 4. 命名与本地化约定

- **类名**:`<原版类名>Fragment` → 如 `FiendFire` → `FiendFireFragment`(类名内部用,不影响显示)
- **title / titleZh**:显示名**不带后缀**,英文直接取原版名(`Fiend Fire`);中文直接取原版中文名(`恶魔之焰`)。" Fragment"/" 碎片" 后缀已按用户要求移除(2026-08-09,拼接卡文字过于繁琐)。
- **id**(= 本地化 key 前缀):`PARTSMITH-` + 类名 CamelCase→SCREAMING_SNAKE
  - `FiendFireFragment` → `PARTSMITH-FIEND_FIRE_FRAGMENT`
  - `OneTwoPunchFragment` → `PARTSMITH-ONE_TWO_PUNCH_FRAGMENT`
- **cards.json 本地化(英文)**:
  ```json
  {
    "PARTSMITH-UPPERCUT_FRAGMENT.title": "Uppercut",
    "PARTSMITH-UPPERCUT_FRAGMENT.description": "Deal 13 damage to an enemy. Apply 1 Weak. Apply 1 Vulnerable."
  }
  ```
- **中文文案**:每张卡的 `titleZh`(中文卡名)与 `descriptionZh`(中文描述)即简体中文本地化。游戏切中文时,写进对应语言文件(如 `localization/zh/` 的 cards.json):
  ```json
  {
    "PARTSMITH-UPPERCUT_FRAGMENT.title": "上勾拳",
    "PARTSMITH-UPPERCUT_FRAGMENT.description": "对一名敌人造成 13 点伤害。施加 1 层虚弱。施加 1 层易伤。"
  }
  ```
- **卡图(已实施,2026-08-09)**:拼卡卡面 = **第一张效果卡的图**(空壳 = 费用卡本图);效果卡 1:1 镜像原版卡,直接复用原版原画。生成脚本为每张卡 emit 一行来源,基类派生全部路径:
  ```csharp
  // UppercutFragment(生成脚本产出)
  protected override CardModel? PortraitSourceCard => ModelDb.Card<Uppercut>();
  ```
  `EffectCardModelBase` 基类从 `PortraitSourceCard` 派生 `PortraitPath`(原版 atlas `atlases/card_atlas.sprites/ironclad/uppercut.tres`)、`BetaPortraitPath`、拼图用 `SplicePngPath`/`SpliceBetaPngPath`(原版独立 PNG `packed/card_portraits/ironclad/uppercut.png`,Blaze/Outrage 无普通 PNG 回退 beta)。JSON 每张卡的 `portraitFrom` 字段给出要引用的原版类型名。奖励界面里效果卡同样显示原版图。
  - **多效果拼图(已实施)**:拼卡按效果卡数量实时拼图,**[card_art_splicing_plan.md](card_art_splicing_plan.md)** 已实现(hook = `CostCardModelBase` override `CustomPortrait`,运行时 `Image.BlitRect` 切片合成 Texture2D,切片源 = 原版卡独立 PNG)。规则:1 张=整图;2 张=中间分开;3 张=三等分;≥4 张=维持三拼。5 张 demo 卡手动指到 `StrikeIronclad`/`DefendIronclad`/`Inflame`/`BattleTrance`。

## 5. 数据分布

| 稀有度 | 数量 | 点数范围 |
|---|---|---|
| Common | 20 | 3–12 |
| Uncommon | 37 | 4–16 |
| Rare | 25 | 5–17 |
| Ancient | 2 | 9、18 |

## 6. 交给另一个 AI 的实现要点

生成每张效果卡时需要:

1. **每个卡生成一个 `EffectCardModelBase` 子类**,构造函数 `base(0, CardType.X, CardRarity.Y, TargetType.Z)`——效果卡**能量费用固定为 0**(它从不单独打出,只被拼到费用卡上)。
2. **`PointCost`** override 返回 `pointCost`。
3. **`ExecuteEffect`**:把 `onPlayCode` 逻辑照搬,但**把 `this` / `base.Owner` / `base.DynamicVars` 替换为宿主拼卡**——攻击/施加效果的来源一律用 `cardPlay.Card`(可参照现有 `StrikeFragment` 写法)。
4. **关键词转移**:`keywords`(Exhaust/Ethereal 等)通过 `CanonicalKeywords` override 设在效果卡上,**且拼接时要把效果卡的关键词转移到宿主费用卡上**(在 `SpliceController.AttachEffect` 里实现)。
5. **稀有度保留**用于奖励权重,`rarity` 字段照用。
6. **Power 卡注意**:作为效果卡,拼卡每次打出都会重新施加该 Power(不是一次性)——这是"效果与原版一致"的自然结果,若不符合预期需另行处理。
7. **卡图**:override 一行 `protected override CardModel? PortraitSourceCard => ModelDb.Card<原版类型>();`(`portraitFrom` 给出原版类型名);`PortraitPath`/`BetaPortraitPath`/拼图 PNG 全由基类 `EffectCardModelBase` 派生,详见 §4。拼卡卡面自动取第一张效果卡的图,奖励界面也显示原版图,无需新做素材。**多效果拼图**(1/2/3 张卡面合成)→ `CostCardModelBase` override `CustomPortrait` 走合成方案,见 [card_art_splicing_plan.md](card_art_splicing_plan.md)。
8. **中文本地化**:除英文 cards.json 外,把 `titleZh`/`descriptionZh` 填进中文语言文件的 cards.json(见 §4)。
9. **升级(upgrade 字段,见 §9)**:`vars` 用增量——`ExecuteEffect` 里按 `cardPlay.Card.IsUpgraded` 在基础值上加增量(如 `Damage.BaseValue + (IsUpgraded ? 2 : 0)`),**绝不能调用效果卡自身的 `OnUpgrade`/`UpgradeValueBy`**(效果卡是 ModelDb 共享单例,直接改会跨战斗/跨拼卡泄漏,同 Rampage 处理)。`addKeywords`/`removeKeywords` 升级时转移到宿主拼卡;`behavior` 行为性升级在 `ExecuteEffect` 里用 `if(cardPlay.Card.IsUpgraded)` 分支;**`cost` 已废弃(2026-08-09 用户决策,见 §9.2)**:11 张"原版仅减费"卡的升级改为"本回合获得 1 点能量",实现集中在 `EffectAttachmentModifier`(基类 `UpgradeEnergyGain` 标记,OnPlay 统一发放 + ModifyDescription 追加 "Gain 1 Energy." 描述,见 §9.2)。升级后的描述随动态 UI 用 `{X:diff()}` 占位符自动渲染(见 §8)。

## 7. 重新生成

JSON 由脚本 `C:/Users/moon side/.claude/jobs/41c309c0/tmp/build_final.py` 从反编译产物生成:

```
python build_final.py    # 读 ironclad_raw.json → 写 partsmith_effect_cards.json
```

- 中间数据 `ironclad_raw.json`(90 张全量提取)由 `extract_cards.py` 生成。
- 游戏升级后,重新跑 `.tools/decompile` 反编译,再跑这两个脚本即可更新。
- 若修改点数规则/排除清单,改 `build_final.py` 顶部的 `FEE_MAP` / `RARITY_BONUS` / `EXCLUDE`。

## 8. 已知注意点

- **描述来源**:`description`/`descriptionZh` 已改为**原版占位符文本**(直接从游戏 pck 的 `localization/eng|zhs/cards.json` 提取,含 `{Damage:diff()}`/`{Block:diff()}`/`{Power:diff()}` 等)。描述里的数值随目标/升级动态显示,不再写死。少量卡因不贴合"碎片拼卡/单机"语境做了人工改写(见下)。
- **EffectCard 与费用卡兼容性**:效果卡 `target` 保持原版。攻击类效果卡拼接后,拼卡需要目标(宿主卡的 `TargetType` 会由效果派生,已在 `CostCardModelBase` 实现)。
- **X 费用卡已排除**(Whirlwind/Cascade),因费用无法映射;若以后要收,需单独定点数规则。
- **描述里的动态数值由占位符渲染**(见 `proposal_effect_card_dynamic_damage.md`):`RefreshPreviewForHost` 用宿主拼卡上下文(力量/目标易伤/虚弱/拼卡升级)刷新效果卡 DynamicVars 的预览值。升级后的数值由 `dynamicVars` 基础值 + `upgrade.vars` 增量实时叠出,**本文件不含升级后的完整描述**。
- **已改写的 6 张卡**(`BodySlam`/`DemonicShield`/`ExpectAFight`/`TearAsunder` 因 `{CalculatedX}` 占位符在共享单例上只能算基础值、不体现动态总量,删掉了 `{InCombat:(…)}` 附注;`Blaze`/`Outrage` 因原版是多人语境措辞,改为单机适用措辞)。
- **重做的卡(2026-08-10 用户决策:效果卡不做忠实原版)**:
  - `StompFragment`(践踏,原 `StampedeFragment` 改名):重做为「对全体造成 base(0,升级 4)+ 本回合每打出一张攻击牌 +4」,点数 3。已修自计数 bug(不含自身)。
  - `StampedeFragment`(惊逃,新移植,点数 11):保留原版 StampedePower 行为(回合结束时随机自动打出 N 张手牌攻击,可叠层),原版"费用-1"升级改为能量增益(`UpgradeEnergyGain=1`,见 §9.2)。
  - **注意基础游戏英文名**:践踏=**Stomp**、惊逃=**Stampede**(与直觉相反),取卡图/移植务必对应——详见 api-notes.md §2.4。

## 9. 升级数据(upgrade 字段)

由脚本 `supplement_upgrade_data.py` 从 `onUpgradeCode`(=原版 `OnUpgrade` 方法体,已核对与反编译源码一致)生成,追加到每张卡。

### 9.1 结构

```json
"upgrade": {
  "summary": "Upgrade: damage +2, max HP +1",   // 人类可读摘要(英文 var 标签 + 中文行为说明)
  "vars": { "Damage": 2, "MaxHp": 1 },          // 各 DynamicVar 升级增量
  "addKeywords": [],                            // 升级新增关键词(需转移到宿主拼卡)
  "removeKeywords": ["Exhaust"],                // 升级移除关键词
  "cost": null,                                 // 已废弃(见 §9.2);效果卡恒 0 费,原版"费用 -1"不落地,全置 null
  "energyGain": 1,                              // 11 张原减费卡的替代升级效果:升级拼卡打出时本回合获得 N 点能量;无则缺省
  "behavior": null | "字符串"                    // 行为性升级说明(含能量增益);无则 null
}
```

### 9.2 语义与约束

- **`vars` 是增量,不是绝对值**:升级后值 = `dynamicVars` 对应 var 的基础值 + `vars[name]`。写成增量是因为它直接从原版 `UpgradeValueBy(n)` 提取,不依赖 `PowerVar` 等丢失变量名的 JSON 表示。**注意 `PowerVar` 在 `dynamicVars` 里只有 `["7m"]` 没有名字**,而 `upgrade.vars` 的键用的是 `onUpgradeCode` 里的真实名(如 `StrengthPower`/`CrimsonMantlePower`/`Power`),两者需靠上下文对应。
- **`cost`(11 张,2026-08-09 用户决策已废弃)+ `energyGain` 替代**:原版这些卡升级仅减费 -1、效果不变(多为 Power/技能)。效果卡恒 0 费,减费无法直接转译 → **升级增益改为「本回合获得 1 点能量」**(`PlayerCmd.GainEnergy(1, player)`;加的是**当前能量**,不影响下回合开始时的能量,已核实 `PlayerCombatState.GainEnergy` 只改当前 `Energy`)。JSON 中 `cost` 已全部置 **null**,替代值记在结构化字段 **`upgrade.energyGain`**(=1)。11 张:Barricade / BodySlam / Corruption / DarkEmbrace / ExpectAFight / Havoc / Hellraiser / InfernalBlade / Stampede / Tank / Unmovable。原版减费记录保留在 `onUpgradeCode`。
  - **实现(2026-08-09 落地)**:基类 `EffectCardModelBase` 加 `public virtual int UpgradeEnergyGain => 0`,生成脚本对 11 张卡 emit `public override int UpgradeEnergyGain => 1`;发放集中在 `EffectAttachmentModifier.OnPlay`——`if (UpgradeEnergyGain != 0 && cardPlay.Card.IsUpgraded) await PlayerCmd.GainEnergy(...)`,**在效果执行之前**(让 Havoc 自动打出的牌也能用上这 1 点能量)。
  - **描述显示(2026-08-09)**:`EffectAttachmentModifier.ModifyDescription` 在宿主拼卡**已升级**(或升级预览 `UpgradePreviewType == Combat`)时,在效果文本末尾追加能量行,措辞参考原版 "Gain {Energy:energyIcons()}." → 本地化键 `cards.PARTSMITH_UPGRADE_ENERGY`:eng `Gain {energy:energyIcons()}.`、zhs `获得 {energy:energyIcons()} 点能量。`(渲染原版铁甲战士能量图标,prefix 取本地角色 CardPool=IroncladCardPool)。效果卡奖励界面(无宿主)不显示。
- **`addKeywords` / `removeKeywords`**:原版 `AddKeyword`/`RemoveKeyword`。升级后要**转移到宿主拼卡**(同拼接时的关键词转移)。
- **行为性升级(4 张)**:`onUpgradeCode` 为 `// no OnUpgrade...` 占位,实际升级写在原版 `OnPlay` 的 `if(base.IsUpgraded)` 分支,效果卡架构下无法用 `vars` 表达:
  | 卡 | `behavior` |
  |---|---|
  | Armaments | 升级后升级手里全部可升级的牌(不再选 1 张) |
  | PrimalForce | 转化的巨岩(GiantRock)是升级版 |
  | Stoke | 加入手牌的牌全部升级 |
  | TrueGrit | 由随机改为选择 1 张手牌消耗(另有 `vars.Block +2`) |

### 9.3 生成/重生成

- 修改 `supplement_upgrade_data.py` 后重跑:`python supplement_upgrade_data.py`(读 `partsmith_effect_cards.json`,就地更新)。
- 若 `build_final.py` 重新生成 JSON,需再跑一次本脚本补回 `upgrade` 字段。
