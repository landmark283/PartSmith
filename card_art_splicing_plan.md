# PartSmith 拼卡卡图拼接方案(简要)

> 配套:`partsmith_effect_cards.json` / `partsmith_effect_cards.md`。
> 现状:拼卡卡面 = 第一张效果卡的图(`CostCardModelBase.PortraitPath`)。本方案把它升级为**按效果卡数量实时拼图**。
> 全部技术点已对照反编译源码核实(见 §2 出处)。

## 1. 显示规则(用户定稿)

| 效果卡数 | 卡面显示 |
|---|---|
| 0 | 空白占位(现状不变) |
| 1 | 该效果卡对应的原版卡图,整张 |
| 2 | 从中间分开:左半边 = 第一张图,右半边 = 第二张图 |
| 3 | 三等分:左 / 中 / 右各取第 1 / 2 / 3 张的对应切片 |
| ≥4 | 不再拼图,维持三张拼图(取前 3 张) |

## 2. 关键机制(已核实)

- **显示入口**:`NCard.UpdatePortrait()` → `Model.Portrait`(返回 `Texture2D`)。
- **`CardModel.Portrait` 是非 virtual 属性**(`CardModel.cs:157`,内部 `ResourceLoader.Load<Texture2D>(PortraitPath)`),C# 子类**无法直接 override**。
- **BaseLib 已留好 hook**:`CustomCardPortrait` patch(`BaseLib.Abstracts.CustomCardPortrait.cs`)拦截 `CardModel.Portrait` getter —— 当 `CustomCardModel.CustomPortrait != null` 时直接返回它。`CustomCardModel.CustomPortrait` 是 **virtual `Texture2D?`**,可自由 override。
- **原画有两个来源**(`ImageHelper.GetImagePath` = `"res://images/" + 路径`):
  - 独立 PNG:`res://images/packed/card_portraits/{池}/{entry}.png` —— 卡面原画独立文件,`HasPortrait` 就是查它(`CardModel.cs:153`),原版卡必有。
  - atlas 子图:`res://images/atlases/card_atlas.sprites/{池}/{entry}.tres` —— `PortraitPath` 用的就是它。
- **切片源选独立 PNG**:它是纯图片,`Texture2D.GetImage()` 拿到的就是整张原画,`Image.BlitRect` 直接切;atlas 是自定义 sheet 子资源(`AtlasManager` / `TpSheetTexture`),`GetImage()` 行为不确定,不冒这个险。

## 3. 拼接实现:运行时合成一张 Texture2D

新增静态类 `CardArtSplicer`,产出**一张完整卡面 Texture2D**,整个引擎链路(`Portrait` getter → NCard)无需任何改动。

```csharp
// 伪代码思路(具体语法按工程实际调整)
static class CardArtSplicer
{
    // fx: 已拼效果卡(有序)。0 张 → null;1 张 → 原画整张;
    // 2 张 → 各取一半;3 张 → 三等分;≥4 → 同 3(取前 3)
    public static Texture2D? Build(IReadOnlyList<CardModel> fx)
    {
        if (fx.Count == 0) return null;

        int n = Math.Min(fx.Count, 3);                       // 最多 3 片
        var src = new List<Image>(n);
        foreach (var e in fx.Take(n))
        {
            var vanilla = ModelDb.Card<???>();               // 效果卡的 portraitFrom 原版类型
            string png = ImageHelper.GetImagePath(
                $"packed/card_portraits/{vanilla.Pool.Title.ToLowerInvariant()}/{vanilla.Id.Entry.ToLowerInvariant()}.png");
            var tex = ResourceLoader.Load<Texture2D>(png, null, ResourceLoader.CacheMode.Reuse);
            src.Add(tex.GetImage());                          // 纯 PNG → 整张原画,安全
        }

        if (n == 1) return src[0].;                          // 单张直接复用(可改走 Portrait)

        int w = src[0].GetWidth(), h = src[0].GetHeight();   // 原版战士原画同尺寸
        var composite = new Image();
        composite.Create(w, h, false, Image.Format.Rgba8);   // 透明底
        for (int i = 0; i < n; i++)
        {
            int x0 = w * i / n, x1 = w * (i + 1) / n;        // 第 i 片竖直区域
            composite.BlitRect(src[i],
                new Rect2I(x0, 0, x1 - x0, h),                // 取第 i 张图的对应切片
                new Vector2I(x0, 0));
        }
        return ImageTexture.CreateFromImage(composite);
    }
}
```

- **缓存**:签名 = 效果卡 `Id.Entry` 有序拼接。签名变才重建,否则复用 `ImageTexture`。
- **尺寸假设**:铁甲战士原画全同尺寸;若未来接入异尺寸卡,按"每片绘制到宽度比例区域"即可(先统一 Resize 或按区域绘制)。

## 4. 挂载与刷新

在 `CostCardModelBase` override `CustomPortrait`(替代/并存现有 `PortraitPath` 覆写):

```csharp
public override Texture2D? CustomPortrait
{
    get
    {
        var fx = SpliceController.AttachedEffects(this).ToList();
        if (fx.Count == 0) return null;                     // 空壳 → 走原路径占位
        string key = string.Join(">", fx.Select(e => e.Id.Entry));
        if (_artCache == null || _artCacheKey != key)
        {
            _artCacheKey = key;
            _artCache = CardArtSplicer.Build(fx);
        }
        return _artCache;
    }
}
```

- **刷新**:`NCard._Ready` 和 `NCard.Reload()` 都会调 `UpdatePortrait()`(`NCard.cs:1244`)。我们已 patch `NCard.Reload`(显示点数角标),拼图搭同一班车——Reload 后 `Model.Portrait` 重新走 getter,自动取最新合成。
- 拼卡拼接后通常不在屏上(在牌组),下次上手/展示是新建 NCard → 天然取最新。

## 5. 副作用与注意点

- **设了 `CustomPortrait` 后 `PortraitPath` 会变空串**:BaseLib 的 `CustomCardPortraitPath` patch 在 `CustomPortrait != null` 时让 `PortraitPath` 返回 `CustomPortrait.ResourcePath`(运行时 ImageTexture 无资源路径)。已核实消费者:
  - `ModelDb.cs:497` 只 `_ = allCard.AllPortraitPaths`(求值) → 无害;
  - `ArtConsoleCmd`(开发控制台 art 命令)→ 拼卡上不显示图,可接受;
  - 渲染链路全走 `Model.Portrait`(被 patch 返回合成图)→ 正常。
- **`BetaPortraitPath` 不参与拼图**:保持现状(第一张效果卡的 Beta)。
- 奖励界面 `NTinyCard` 显示的是效果卡 / 费用壳本身(未拼卡),不受影响。
- 性能:合成一次 + 签名缓存;每帧只是字符串比对,开销可忽略。

## 6. 参考

- GitHub 检索未发现专门的"卡图拼接"StS2 mod;最接近的权威参考是 [Alchyr/ModTemplate-StS2 的替换卡图指南](https://raw.githubusercontent.com/wiki/Alchyr/ModTemplate-StS2/Replacing-Base-Game-Text-%26-Images.md),它确认了卡图资源布局(`images/card_portraits/…`、atlas 存储、同路径可覆盖)与替换方法。
- 游戏自身动态换图先例:**Wither**(按升级等级切换 `PortraitPath`/`PortraitPngPath`)、**MadScience**(按类型切换)——证明"路径级换图"引擎原生支持;本方案在其上走**纹理级合成**,是超集。
- 拼接本身是 Godot 标准 `Image` 合成(`BlitRect` + `ImageTexture.CreateFromImage`),不依赖现成 mod。

## 7. 实施清单

1. 新建 `CardArtSplicer`(静态类:取原画 PNG → 切片合成 → 缓存)。
2. `CostCardModelBase` override `CustomPortrait`(含签名缓存)。
3. 验证点(游戏内):
   - 空壳 → 占位图不变;
   - 拼 1 张 → 显示该效果卡原版原画(整张);
   - 拼 2 张 → 左半是第一张图、右半是第二张图;
   - 拼 3 张 → 三片从左到右;
   - 拼 4 张 → 维持 3 片拼图。

---

## 8. 实施记录(2026-08-09,已部署待游戏内验证)

- **全部落地**:`CardArtSplicer`(`src/PartSmith/PartSmithCode/Cards/Splicing/CardArtSplicer.cs`,签名缓存 + `Image.BlitRect` 切片 + `ImageTexture.CreateFromImage`,`Image.CreateEmpty` + 透明 Fill 防脏像素)+ `CostCardModelBase.CustomPortrait` override(0 张→null 走占位;1/2/3/≥4 张合成;带实例级签名缓存)。
- **卡图来源重构**:效果卡不再逐卡 override `PortraitPath/BetaPortraitPath`(原来 2 行/卡),改由 `EffectCardModelBase` 基类提供 `protected virtual CardModel? PortraitSourceCard`(生成脚本只 emit 一行 `ModelDb.Card<原版>()`),基类派生 `PortraitPath`(原版 atlas)/`BetaPortraitPath`/`SplicePngPath`/`SpliceBetaPngPath`(原版独立 PNG)。5 张 demo 卡(Strike/Block/Guard/Power/Draw)手动指到 `StrikeIronclad`/`DefendIronclad`/`DefendIronclad`/`Inflame`/`BattleTrance`。
- **PortraitPngPath 覆写**:`CardModel.PortraitPngPath` 是 `protected virtual`(CS0114 隐藏警告);覆写为 `protected override` 让效果卡 `HasPortrait` 改按原版卡解析(更正确)。拼图/回退用公开的 `SplicePngPath`/`SpliceBetaPngPath`/`SourcePortraitTexture`。
- **Blaze/Outrage 无普通原画**:pck 里只有 `card_portraits/ironclad/beta/{blaze|outrage}.png`,无普通 PNG(84 卡中仅这 2 张)。拼图时主 PNG 不存在自动回退 beta PNG。
- **回退防空白**:合成失败(理论上不应发生)时 `CustomPortrait` 回退 `fx[0].SourcePortraitTexture`(原版卡 `Portrait`,BaseLib patch 对非 CustomCardModel 放行原始加载);不用 `fx[0].Portrait`(效果卡是 CustomCardModel,其 `Portrait` 走 CustomPortraitPath→mod 里不存在的 PNG→null)。
- **刷新机制**:无需新增 patch——拼卡拼接时不在屏上(在卡组),下次上手/展示新建 NCard 走 `_Ready→UpdatePortrait→Model.Portrait`(被 BaseLib CustomCardPortrait patch 拦截返回合成图)。已在屏卡后续拼卡靠 `NCard.Reload`(点数角标 patch 同班车)→ `UpdatePortrait` 刷新。
- **BlitRect 格式坑(实测踩坑,2026-08-09 已修)**:`Image.BlitRect` 要求两图**同格式**,否则 `ERROR: Condition "format != p_src->format"` 静默失败 → 合成卡面空白。原版卡原画独立 PNG 多为**不透明 RGB8**(无 alpha),合成底是 RGBA8 → 必挂。修复:每张源图 `img.Convert(Image.Format.Rgba8)` 后再 BlitRect(在 Build 的 GetImage 之后)。游戏日志在 `%APPDATA%/SlayTheSpire2/logs/godot.log`(user:// 被游戏重定向到 Roaming/SlayTheSpire2)。
- **效果卡奖励界面无卡图(2026-08-09,已修)**:效果卡是 `CustomCardModel`、`CustomPortrait` 恒 null、`CustomPortraitPath`(PartSmithCard 设的 mod PNG,不存在)→ BaseLib `CustomCardPortrait` Prefix(`BaseLib.Abstracts.CustomCardPortrait.cs`)把 `CardModel.Portrait` 设成 `ResourceLoader.Load(CustomPortraitPath)`=null → 奖励选择屏(`NCardRewardSelectionScreen` 用 `NCard.Create`→`NCard.UpdatePortrait`→`Model.Portrait`)无卡图。修复:`EffectCardModelBase` override `CustomPortrait => PortraitSourceCard?.Portrait`(原版卡非 CustomCardModel,patch 放行原始加载 → 返回原版 atlas 卡面)。**区别于拼图**:拼图切片源是独立 PNG(`SplicePngPath`),本属性不影响 `CardArtSplicer`。
- **去碎片后缀(2026-08-09,用户定)**:显示名不带 " Fragment"/" 碎片"(拼接卡名=效果名逗号连接,现在只显原版名)。JSON title/titleZh 全 84 张清洗(脚本 `tmp/strip_fragment.py`)+ 重跑生成脚本同步本地化;5 张 demo 卡 eng 去后缀、zhs 补中文条目。类名仍保留 `Fragment` 后缀(内部用,不影响显示)。
- **精英怪奖励(2026-08-09,用户定)**:精英 = 1 费用卡 + 2 效果卡(`RewardInjectionPatch` 对 `RoomType.Elite` 再插一个 `CreateEffectCardReward`;两个 SpliceReward 同 `RewardsSetIndex=6`,稳定排序相邻)。普通小怪仍 1+1。
- **构建 0 错 0 警**,4 文件(dll/pdb/json/pck)已部署,游戏已启动。验证点见 §7 清单。

---

## 9. 调试命令:手牌生成拼卡 + 升级(2026-08-11)

控制台开关:`` ` ``(backtick,即 `~` 那个键)或 `Shift+8`;`Esc` 关闭。所有命令都要一局游戏在进行中。

### 9.0 完整命令参考

**`parttest`(PartSelfTestCommand,自动化自测/拼卡工具)**

| 子命令 | 作用 |
|---|---|
| `parttest room [<encounterId>]` | 直达测试战斗房间(默认 `KNIGHTS_ELITE`,3 敌);`parttest encounters` 看全部合法 id |
| `parttest maxhp` | 当前战斗所有敌人血量顶到 999999999(≈打不死) |
| `parttest make [<costCardId>] <effectId[,effectId...]>` | 造拼卡并塞进手牌(战斗)或牌组(非战斗);宿主默认 `Scrap`,可显式指定费用卡;绕过点数容量校验 |
| `parttest info <handIdx>` | 打印手牌第 handIdx 张卡的信息(标题/类型/关键词/点数/星费/描述) |
| `parttest encounters` | 列出全部合法遭遇 id |
| `parttest library [effect\|cost\|hunter_effect\|hunter_cost\|wang_effect\|wang_cost\|all]` | 图鉴自检:列出各池在图鉴里应显示的卡及可见状态(LOCKED/NOT_SEEN/VISIBLE),默认 effect |

id 匹配容错:带不带 `PARTSMITH-` 前缀都认(`SLICE_FRAGMENT` / `PARTSMITH-SLICE_FRAGMENT` 均可)。

**`partsplice`(SpliceTestCommand,往牌组拼接,走正常容量校验)**

| 子命令 | 作用 |
|---|---|
| `partsplice shell` | 往牌组加一张空壳费用卡(Scrap) |
| `partsplice attach <deckIndex> <effectId>` | 把效果卡拼到牌组第 deckIndex 张卡上(超容量报错) |
| `partsplice list` | 列出牌组里的费用卡与已拼效果 |

### 9.1 生成拼卡进手牌

```
parttest make SLICE_FRAGMENT
```

- `parttest`(`PartSelfTestCommand`)默认用 `Scrap`(0 费壳)当宿主,跳过点数容量校验,把效果卡拼上去再塞进卡堆。
- **战斗中 → 加进手牌**;地图/非战斗 → 加进牌组。
- ⚠ 不要用原版 `card PARTSMITH-SLICE_FRAGMENT hand` 加效果卡——那是**裸效果卡**(无宿主),不能正常打出。测拼卡必须走 `parttest make`。
- 效果卡 id 用 SCREAMING_SNAKE,带不带 `PARTSMITH-` 前缀都行(2026-08-11 起命令做了容错):`SLICE_FRAGMENT` / `PARTSMITH-SLICE_FRAGMENT` 都认。常用:`SLICE_FRAGMENT`(猎人割裂)/ `BLUDGEON_FRAGMENT`(战士重锤)/ `DEVASTATE_FRAGMENT`(储君葬送)等;也可用 `partsplice` 系列(shell/attach/list)往牌组拼。

### 9.2 查看手牌某张卡

```
parttest info 0
```

打印标题 / 类型 / 关键词 / 点数 / 星费 / 描述(描述走手牌预览,会实时反映升级后的数值)。

### 9.3 升级(原版命令,可无限连发)

```
upgrade <手牌位置>
```

- 原装 `UpgradeCardConsoleCmd`,调 `CardCmd.Upgrade`;0 = 左手边第一张。
- 壳 `MaxUpgradeLevel = int.MaxValue` 后,它的"已升满"守卫永不触发 → **可反复升级**,标题 `+N`。
- 效果卡按 `MaxUpgradeLevels` 惰性缩放,统一封顶 1 级(割裂/重锤/葬送原无限叠层已于 2026-08-11 还原,不再每级一份增量;壳等级继续涨,效果只吃一级)。

### 9.4 一条龙测试套路

```
parttest room                    ← 直达测试战斗(默认 KNIGHTS_ELITE,3 敌)
parttest maxhp                   ← 敌人血量顶到 999999999,打不死
parttest make SLICE_FRAGMENT     ← 拼卡进手牌
parttest info 0                  ← 确认位置和初始数值(割裂 6 伤)
upgrade 0                        ← 升 1 级;可连发
upgrade 0
parttest info 0                  ← 割裂显示 9(6+3;效果只升 1 级封顶,再连发壳等级涨但伤害不变)
```
