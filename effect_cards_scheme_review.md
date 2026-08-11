# 效果卡方案三问核查:描述清晰度 / 动态数值 / 升级文本

> 对象:`partsmith_effect_cards.md`(84 张效果卡数据说明)+ `proposal_effect_card_dynamic_damage.md`(动态数值提案)。
> 方法:逐条对照反编译源码与游戏 pck 核实,非猜测。
> 结论先行:**基础效果能描述清楚、动态数值机制可行但未落地、升级文本在当前设计下不正确(文档 §8/§9 的"自动渲染"断言有误)**。

---

## 结论速览

| 问题 | 结论 | 关键缺口 |
|---|---|---|
| Q1 效果能否描述清楚 | 基础效果 ✓(攻击/技能准确);Power 卡通用 △(与原版一致);**升级后行为 ✗** | 4 张行为升级卡(Armaments/TrueGrit/PrimalForce/Stoke)无 `{IfUpgraded:}` 模板 |
| Q2 攻击/防御能否动态计算 | **机制可行 ✓**(全链路已核实),但当前 JSON 是静态数字 | 占位符迁移 + `RefreshPreviewForHost` 未实施;计算卡乘数函数不在 JSON |
| Q3 升级后文本是否正确 | **不正确 ✗**(按现状设计) | ①数值:canonical 效果卡恒基础值,升级增量进不了显示;②行为文本无模板;③11 张 cost -1 未决策 |

---

## 一、机制事实(已核实,理解结论的前提)

**1. 升级文本在引擎里不是"独立的升级字符串"**,而是两种机制叠加:

- **数值**:升级时 `UpgradeInternal()` → `OnUpgrade()` → 各 DynamicVar `UpgradeValueBy(n)` 改 **BaseValue**(实例级突变),描述里 `{Damage:diff()}` 渲染的就是 BaseValue 派生值。
- **文本**:同一字符串里的条件段 `{IfUpgraded:show:升级文本|基础文本}`,由 `IfUpgradedVar` + `UpgradeDisplay` 控制取哪支。
- 证据:从游戏 pck 直接扫出大量 `{IfUpgraded:show:...}`(如 `{IfUpgraded:show:ALLE deine Handkarten|eine deine Handkarten}` 正是 Armaments 的"全部手牌|一张手牌"),`diff()` 出现 7300+ 次。

**2. 拼卡描述链路(决定了上面两条都不适用于效果卡)**:

- `GetDescriptionForPile`:描述字符串 + `DynamicVars.AddTo(description)`(**本模型自己的 vars**)+ `IfUpgradedVar(**本模型自己的 IsUpgraded**)` → `GetFormattedText()`。
- BaseLib 的 `CustomizeDescription` 事件在 `GetFormattedText()` **之后**触发 → `EffectAttachmentModifier.ModifyDescription` 把 `effect.GetEffectDescription(target)` 拼进宿主描述。
- `GetEffectDescription` = 效果卡自己的 `GetDescriptionForPile`,返回**占位符已解析完毕的最终字符串**。
- 推论:效果文本里的 `{X:diff()}` **永远按效果卡 canonical 实例的 vars 和 IsUpgraded 渲染** —— IsUpgraded 恒 false、BaseValue 恒基础值(效果卡是 ModelDb 共享单例,§9 正确地禁止 mutate 它防泄漏)。

---

## 二、Q1:效果能否描述清楚?

84 张全量审计结果:

- **攻击/技能卡基础描述:清晰准确**。逐卡对照过 `onPlayCode`,数值/目标/关键词一致(上勾拳 13 伤+虚弱+易伤、恶魔之焰 消耗手牌×7、血墙 失2血+16防 等)。
- **Power 卡:通用描述"获得 X 层 <名>",与原版一致**(Inflame=获得 2 点力量 等已知名词无歧义;Tank/Unmovable 等自定义 power 的机制靠 power 自身 tooltip,卡面文本本来就是一句话)。
- **缺口 1(行为升级卡)**:4 张卡升级改变**行为**,`upgrade.behavior` 只是人类笔记,**不是可渲染模板** → 升级后的卡面文本无法表达行为变化:
  - Armaments 升级:升级手中全部可升级牌(不再选 1 张)
  - TrueGrit 升级:由随机改为选择 1 张消耗
  - PrimalForce 升级:变形的巨岩是升级版
  - Stoke 升级:加入手牌的牌全部升级
- **缺口 2(升级数值)**:静态描述只描述基础效果,升级数值不在文本里(§8 有意为之,见 Q3)。

**结论**:基础效果能描述清楚;升级后的效果**不能**。

---

## 三、Q2:攻击/防御值能否动态计算?

**机制可行,全链路已核实**:

1. 占位符 `{Damage:diff()}` / `{Block:diff()}` → `HighlightDifferencesFormatter` → `(int)PreviewValue` + 绿/红高亮(Preview>Enchanted 绿,反则红)。
2. PreviewValue 由 `UpdateCardPreview` 算:`DamageVar.UpdateCardPreview` = BaseValue → 附魔 → `Hook.ModifyDamage`(力量/目标易伤/虚弱/遗物全进来);`BlockVar` 同理走 `Hook.ModifyBlock`。
3. 计算伤害卡(BodySlam / PerfectedStrike / Bully 等):`CalculatedVar.Calculate(target)` = 基础 + 额外 × 乘数(乘数函数来自战斗状态,如格挡值 / '打击'牌数),预览数学成立。
4. 非战斗场景(卡组/地图)走 `runGlobalHooks=false` 回退基础值,与原版一致。

**但当前状态没落地**:

- 描述还是**静态数字**(提案 §1),`{X:diff()}` 迁移未实施;
- 效果卡需在 C# 定义 `CanonicalVars`(DamageVar/BlockVar/CalculatedDamageVar **+ WithMultiplier**),JSON 的 `dynamicVars` 有基础值但**没有乘数函数**(它在 `onPlayCode` 里,导入方要从代码提取);
- 需 `RefreshPreviewForHost`(提案 §5.3)用宿主上下文刷新 canonical 效果卡的 PreviewValue;
- `PowerVar` 的 name 在 JSON 里丢失(`["3m"]` 无名字,§9 已标注),但 Power 不做动态数值显示,影响小。

**结论**:能动态计算,但必须按提案实施迁移 + 每卡补 CanonicalVars/占位符;当前 JSON 不含动态描述。

---

## 四、Q3:升级后文本显示是否正确?

**不正确**(按当前设计)。三处缺口 + 一处机制矛盾。

### 缺口 1:数值(核心)

- 效果卡 canonical 实例 BaseValue 恒基础值;`DamageVar.UpdateCardPreview` 只用 `BaseValue`(无升级概念)。
- 宿主拼卡升级后(`host.IsUpgraded=true`),效果卡描述里的 `{Damage:diff()}` 仍渲染**基础值**。
- §8/§9 声称"升级后的描述随动态 UI 用 `{X:diff()}` 占位符**自动渲染**" —— **这是误判**:占位符只解决"随目标/力量动态",不解决"随升级"。
- 机制矛盾:§9 禁止 mutate canonical(正确,防泄漏),但显示路径又依赖 canonical vars → **唯一出路是走 PreviewValue/EnchantedValue(display-only,专为显示设计)**。

**修复建议(精确到语义,与 vanilla 完全一致)**:
在 `RefreshPreviewForHost` 里,宿主 `IsUpgraded` 时,先设
`EnchantedValue = BaseValue + upgrade.vars[name]`
再 `PreviewValue = EnchantedValue`,然后照常跑 hooks。
- `EnchantedValue` 的语义正是"卡自身的一部分"(升级/附魔都算它,不标绿);
- 结果:升级拼卡显示提升后的数字但**不常驻绿色**;力量/易伤 buff 才标绿 —— 与原版升级卡完全一致。
- 增量来源:JSON `upgrade.vars`(按 var 名取,如 Uppercut 的 `Power+1` → 需映射到 dynamicVars 里的显示 var)。

### 缺口 2:行为文本

- 4 张行为升级卡(见 Q1)无 `{IfUpgraded:show:...}` 模板 → 升级后行为文本显示不出。
- 修复:给这些卡的 description 补条件段(vanilla 同款语法),示例:
  - Armaments:`{IfUpgraded:show:升级你手中的所有可升级牌。|升级你手中的一张牌。}`
  - TrueGrit:`{IfUpgraded:show:消耗你手中的一张牌。|消耗你手中一张随机牌。}`
  - PrimalForce / Stoke 同理。

### 缺口 3:费用

- 11 张卡原版升级是能量 -1。§9 标注"是否映射到升级拼卡后拼卡费用 -1 是后续决策"。
- 若不实现,升级后的拼卡**费用不减**,与原版体验不一致。
- 需明确决策并写入实现要点。

### 附:两个结构性问题

- **升级流程本身**:宿主拼卡升级(Smith/奖励)走宿主 `OnUpgrade`;多效果宿主**一张升级 level 同时应用全部效果增量**。需明确"合并升级"设计,否则数值/文本都无法对。
- **奖励屏升级预览**:`GetDescriptionForUpgradePreview` 同样走拼卡描述链路,效果段同样显示基础值 —— 与缺口 1 同根,一并修。

---

## 五、建议的文档/数据层改动(交给导入方 AI)

1. **JSON 每卡新增 `upgrade.description`**:行为升级卡的 `{IfUpgraded:show:升级文本|基础文本}` 完整描述模板(其余卡可留空,数值由动态渲染)。
2. **JSON `dynamicVars` 补全**:
   - PowerVar 带名字(`["3m"]` → `{"type":"PowerVar","args":["StrengthPower","3m"]}`);
   - 计算伤害卡补 `multiplier` 说明(或明确"乘数逻辑从 onPlayCode 提取")。
3. **提案增补**:`RefreshPreviewForHost` 增加"宿主升级增量注入 EnchantedValue + PreviewValue"两段式(见 §4 缺口 1 修复建议)。
4. **§8/§9 表述修正**:删掉"升级后描述自动渲染"的断言,改为"升级数值需按 §4 缺口 1 方案注入预览;行为文本需 upgrade.description 模板"。
5. **费用升级决策**:明确 11 张 cost -1 是否映射拼卡费用,写入实现要点。

---

## 附:核实依据(反编译源码 / pck)

| 事实 | 位置 |
|---|---|
| 升级 = `UpgradeInternal → OnUpgrade → UpgradeValueBy`(实例级突变 BaseValue) | `CardModel.cs:2130-2135`、`DynamicVar.cs:143` |
| 描述用同一字符串 + `DynamicVars.AddTo` + `IfUpgradedVar(IsUpgraded)` | `CardModel.cs:1372-1410` |
| `{X:diff()}` → `(int)PreviewValue` + 绿/红 | `DynamicVar.cs:171`(`ToHighlightedString`) |
| PreviewValue 由 hooks 算(不含升级) | `DamageVar.cs:28-46` |
| 计算伤害 `base + extra×multiplier` | `CalculatedVar.cs:76-100` |
| 拼卡描述 = CustomizeDescription 在 GetFormattedText **之后**注入效果卡已解析文本 | `BaseLib/DescriptionOverrides.cs:30-36`、`EffectAttachmentModifier.cs:61-73` |
| 原版大量使用 `{IfUpgraded:show:...}` 换升级文本 | `SlayTheSpire2.pck` 文本扫描(20+ 例,含 Armaments) |
| EnchantedValue = "卡自身的一部分"(升级/附魔归它,不标绿) | `DynamicVar.cs:46-68` 注释 |
