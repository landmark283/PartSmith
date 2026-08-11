# Slay the Spire 2 Mod 开发 — 技术准备调研笔记

> 整理日期:2026-08-08
> 状态:**StS2 仍处 Early Access(EA)**,版本与 mod 结构变动频繁,本笔记基于公开资料与社区文档整理,动手前请以最新官方模板/文档为准。
> 相关:[组装卡牌奖励机制设计](../ideation) —— 本笔记服务的目标项目:卡牌奖励改为"资源零件 × 效果零件 → 合成新卡"。

---

## 0. TL;DR(结论速览)

1. **官方没有正式 mod API 文档。** Mega Crit 提供的是**工具**(内置 mod 加载器、Steam 工坊、官方上传器、自改引擎 MegaDot),具体怎么开发全靠**社区文档**(Alchyr 的模板/BaseLib、wiki.gg、Discord)。
2. **做 mod 基本需要 Godot 环境**(MegaDot = Megacrit 自改的 Godot 4.5.1):C# mod 必须引用 `GodotSharp.dll` 编译,含资源/文本的 mod 要用 Godot 导出 `.pck`。但**核心语言是 C#,不是 GDScript**。
3. **核心工具链**:`.NET SDK 9.0+` + C# IDE(推荐 Rider)+ `MegaDot`(或版本匹配的官方 Godot .NET)+ 模板包 `Alchyr.Sts2.Templates` + 社区框架 **BaseLib**。
4. **最小 mod 形态**:`ModName.json`(manifest)+ `ModName.dll`(代码)+ `ModName.pck`(资源/文本),放进 `<游戏目录>/mods/<ModName>/`,游戏内 `Settings → Mod Settings` 开关。
5. **对本项目最有价值的钩子**(都在 BaseLib 里):`ConstructedCardModel`(程序化定义卡)、`CardModifier`(运行时增删效果)、**自定义卡牌奖励序列化**(自定义奖励流程的落点)。

---

## 1. 官方到底给了什么

### 1.1 官方提供的(有)

| 东西 | 说明 |
|---|---|
| 内置 mod 加载器 | 游戏"生来就支持 mod",从 `<游戏>/mods/` 目录加载;游戏内设置可开关,`-nomods` 启动参数可禁用 |
| Steam 工坊支持 | 2026-06(v0.107.1)起正式接入;支持依赖声明、多人环境 `affects_gameplay` 标记 |
| 官方上传器 | GitHub `megacrit/sts2-mod-uploader`:发布到工坊的官方工具,`workshop.json` 元数据、图片需 <1MB |
| MegaDot | Mega Crit 自改的 Godot 4.5.1 引擎,mod 开发者用它来做开发环境 |

### 1.2 官方没有的(关键)

- **没有正式的 modding API 文档**。社区 wiki.gg 原话:
  > "As STS2 does not have a formal modding API, nearly all of the information here has been gathered and maintained by the modding community."
- **实际上的"API 文档" = 反编译游戏自带的 `sts2.dll`**(用 ILSpy),mod 代码里常出现的命名空间:`MegaCrit.Sts2.Core.Modding`、`MegaCrit.Sts2.Core.Logging`。
- 官方推荐开发路径:下载 MegaDot → 用社区模板 → 反编译 `sts2.dll` 查类型 → 写 C# → 导出 pck → 上传工坊。

---

## 2. 要不要装 Godot?——拆开讲

**结论:需要 Godot 环境,但不需要"用 Godot 做游戏"。**

| 问题 | 答案 |
|---|---|
| 为什么需要 Godot? | ① mod 工程要引用 Godot C# 绑定(`GodotSharp.dll`)编译;② 含资源/本地化文本的 mod 要用 Godot 的 **Export 导出 `.pck`** |
| 装哪个? | 优先 **MegaDot**(Mega Crit 官方自改版);兜底用**与最新 MegaDot 版本匹配的官方 Godot .NET 版** |
| 怎么告诉工程? | 在 `Directory.Build.props` 里配置 `<GodotPath>` |
| 注意 | Godot 需要 `.sln` 工程文件(旧式,不是新的 `.slinx`) |

> 纯 C# 逻辑 mod 理论上可以不碰编辑器导出,但模板与官方示例默认都会走 Godot 导出流程,照做最省事。

---

## 3. 完整工具链清单

| # | 工具 | 用途 | 备注 |
|---|---|---|---|
| 1 | Slay the Spire 2 游戏本体 | 测试 | EA 版,注意版本 |
| 2 | **.NET SDK 9.0+** | 编译 C# mod | 硬性要求 |
| 3 | C# IDE | 写代码 | **Rider 推荐**(注意 `.sln` 格式);VS / VS Code 亦可 |
| 4 | **MegaDot** 或匹配的 Godot .NET | 编译引用 + 导出 pck | 硬性要求(见 §2) |
| 5 | 模板包 | 脚手架 | `dotnet new install Alchyr.Sts2.Templates`,提供 **Character / Content / Empty** 三种模板;另见 NuGet `S2SModTemplate.Templates` |
| 6 | **BaseLib** | 社区框架,省 boilerplate | Steam 工坊订阅(ID **3737335127**)或 GitHub Releases 手动装 |
| 7 | ILSpy | 反编译 `sts2.dll` 当"文档" | 事实标准 |
| 8 | (可选)**STS2 Modding MCP** | 给 AI(如 Claude Code)用的 MCP 服务器 | `elliotttate/sts2-modding-mcp`,153 个工具:查游戏数据、生成 mod 代码、构建、部署;自动识别 Steam 安装路径、自动反编译、自动部署 bridge mod |

---

## 4. Mod 的形态与安装

### 4.1 文件组成(最多三件)

| 文件 | 内容 | 是否必需 |
|---|---|---|
| `ModName.json` | manifest:id、显示名等 | **必需** |
| `ModName.dll` | C# 代码 | 内容 mod 基本必需 |
| `ModName.pck` | 资源/本地化文本(Godot 导出) | 纯代码可省略,模板默认会生成 |

> ⚠️ EA 期结构改过多次:历史版本把 manifest 内嵌为 `mod_manifest.json`,后改为独立的 `<ModName>.json`(json 成为声明 mod 的唯一必需文件,manifest 声明是否带 pck/dll)。**以当前模板生成的结果为准**。

### 4.2 安装位置

- Windows/Linux:`Steam/steamapps/common/Slay the Spire 2/mods/<ModName>/`
- macOS:`SlayTheSpire2.app/Contents/MacOS/mods/<ModName>/`
- Steam 工坊订阅的 BaseLib 落点:`steamapps/workshop/content/2868840/3737335127/BaseLib`

### 4.3 开关与存档

- 游戏内 `Settings → Mod Settings` 管理 mod 开关;`-nomods` 启动参数可完全禁用。
- **modded / 无 mod 存档分开存储**;首启会自动把无 mod 存档复制一份到 modded 存档(有弹窗提示)。

---

## 5. 最小可用 mod 的制作流程(骨架)

综合社区模板(wiki)与实操示例 `jiegec/STS2FirstMod`:

1. 装齐工具链(§3):.NET SDK 9.0+、IDE、MegaDot。
2. 安装模板:`dotnet new install Alchyr.Sts2.Templates`。
3. 建解决方案(建议 **Empty 或 Content** 模板;用 `.sln` 格式)。
4. 引用 BaseLib(工坊订阅或本地 dll 引用)。
5. 写入口:用 `[ModInitializer]` 特性 + `MegaCrit.Sts2.Core.Modding` 命名空间。
6. 加内容:继承 BaseLib 的 `Custom*Model`(游戏几乎所有内容都是 `*Model` 对象,如 `CardModel`)。常用做法是继承模板自带的卡基类(如 Content 模板的 `FoobarModCard`)。
7. 配置 `<GodotPath>`,编译出 `.dll`。
8. 用 Godot Export 导出 `.pck`(有资源/文本才需要)。
9. 写 `ModName.json` manifest + `mod_image.png` 图标。
10. 三个文件放进 `mods/<ModName>/`,游戏内开启,测试。
11. 发布:官方 `sts2-mod-uploader` 推 Steam 工坊。

---

## 6. 卡片系统的结构(本项目关键)

- **游戏内容几乎全是 `*Model` 对象**(`CardModel`、`RelicModel`…),BaseLib 提供可继承的 `Custom*Model` 基类,做法是继承并 override 属性。
- BaseLib 提供的、与本项目直接相关的 API:
  - **`ConstructedCardModel`**:程序化定义卡(可替代 `CustomCardModel` 作为基类)——"按参数拼一张卡"的落点。
  - **`CardModifier`**:运行时给卡增减效果(相当于 StS1 的 CardModifierManager)。
  - **自定义卡牌奖励序列化**:改/替换卡牌奖励流程的钩子。
- **官方没有"卡片拼装"现成 API**,组合逻辑(资源零件 × 效果零件 → 新卡)需要自己在 C# 层实现。

---

## 7. 对本项目(拆零件拼卡)的落地路径建议

1. 用 **Content 模板 + BaseLib** 起项目。
2. **先验证**(里程碑 1):用 `ConstructedCardModel` 在局内按参数生成一张新卡,并成功放进玩家牌组。这一步通了,机制就成立。
3. 再实现**零件池**:资源零件 + 效果零件(手工设计的参数化模板,见设计笔记 §三)。
4. 最后改**奖励 UI**:两段式选择界面(选资源 → 选效果 → 合成)。依赖 BaseLib 的自定义奖励序列化 + 一定量的 UI 工作。
5. 风险提示:改奖励界面是纯 UI 工作,工作量比改数据大;EA 期 API 变动可能让你重写部分 UI 代码。

---

## 8. 风险与注意事项

- **EA 期变动频繁**:教程作者自称 WIP;manifest 结构历史上改过多次;游戏更新可能弄坏 mod。
- **反编译是事实标准**,但注意版权边界(别直接挪用游戏素材/代码到线上发布)。
- **Steam 工坊发布**注意:图片 <1MB、`workshop.json` 元数据、多人环境 `affects_gameplay` 标记、依赖声明(依赖 BaseLib 时 load order 会被强制重排)。
- **社区渠道**(遇到问题先看这里):
  - [Slay the Spire 官方 Discord](https://discord.gg/slay-the-spire) #sts2-modding 频道(社区建议的求助地)
  - [wiki.gg Modding Tutorials](https://slaythespire.wiki.gg/wiki/Slay_the_Spire_2:Modding_Tutorials)
  - [Steam 社区中文开发指南](https://steamcommunity.com/app/2868840/discussions/0/806845425982211616/)

---

## 9. 资料链接汇总(Sources)

**官方**
- 官方上传器:[megacrit/sts2-mod-uploader](https://github.com/megacrit/sts2-mod-uploader)
- Steam 工坊支持更新:[spire-codex 报道](https://spire-codex.com/news/1835871199306777)、[v0.107.1 更新说明](https://sts2.untapped.gg/en/articles/slay-the-spire-2-v01071-patch-notes-main-branch-update-2)

**社区文档 / 模板 / 框架**
- 模板与教程:[Alchyr/ModTemplate-StS2](https://github.com/Alchyr/ModTemplate-StS2) | [Setup](https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup) | [Modding Basics](https://github.com/Alchyr/ModTemplate-StS2/wiki/Modding-Basics)
- 框架:[Alchyr/BaseLib-StS2](https://raw.githubusercontent.com/wiki/Alchyr/BaseLib-StS2/Home.md)
- 教程 hub:[wiki.gg — Slay the Spire 2: Modding Tutorials](https://slaythespire.wiki.gg/wiki/Slay_the_Spire_2:Modding_Tutorials)
- WIP 教程:[GlitchedReme/SlayTheSpire2ModdingTutorials](https://github.com/GlitchedReme/SlayTheSpire2ModdingTutorials)
- 可运行示例:[jiegec/STS2FirstMod](https://github.com/jiegec/STS2FirstMod)
- 中文指南:[Steam 综合讨论 — 杀戮尖塔2 Mod 开发指南](https://steamcommunity.com/app/2868840/discussions/0/806845425982211616/)
- AI 辅助(与本工具直接相关):[elliotttate/sts2-modding-mcp](https://github.com/elliotttate/sts2-modding-mcp)
- NuGet 模板:[S2SModTemplate.Templates 0.2.0](https://www.nuget.org/packages/S2SModTemplate.Templates/0.2.0)

**引擎背景**
- [StS2 弃 Unity 改用 Godot(80.lv)](https://80.lv/articles/slay-the-spire-2-is-sticking-with-godot-leaving-unity-behind)

**已有相似 mod(设计参考)**
- 卡牌融合 MOD(StS2,篝火两卡合一):[3DM](https://dl.3dmgame.com/patch/387171.html)
- 卡牌少女不会受伤(StS1,两卡合一):[Steam 工坊](https://steamcommunity.com/sharedfiles/filedetails/?id=3730579908)
- 三重聚赏 / Triple Reward(StS2,三合一):[Nexus](https://www.nexusmods.com/slaythespire2/mods/721?tab=description)
- SPIRECRAFT(StS2,撕碎卡牌拼卡):[Nexus](https://www.nexusmods.com/slaythespire2/mods/354?tab=description)
- Card editor and Card creator(StS2,局内造卡):[Nexus](https://www.nexusmods.com/slaythespire2/mods/69?tab=description)

---

## 附录:本机环境落地记录(2026-08-08)

> 实战搭建完成。项目骨架在 `src/PartSmith/`,可编译出 dll。以下为可复现的环境与工作流。

### 环境

| 项 | 值 |
|---|---|
| 游戏本体 | `D:\Steam\steamapps\common\Slay the Spire 2`(v0.110.1,EA) |
| sts2.dll / GodotSharp.dll | `...\data_sts2_windows_x86_64\`(编译时直接引用,免装 MegaDot) |
| .NET SDK | `D:\lg\else\mod\.tools\dotnet`(9.0.316,本地安装;**已加入用户 PATH**,新开终端可直接 `dotnet`) |
| Godot 4.5.1 .NET(mono) | `E:\Godot\Godot_v4.5.1-stable_mono_win64\`(`GodotPath` 已指向其 console exe) |
| BaseLib | NuGet `Alchyr.Sts2.BaseLib` 3.4.0(编译用);运行时文件解到 `dist/BaseLib/` |

### 网络坑与离线包源(重要)

- 本机网络 **连不上 `nuget.org` 与 `github.com`**(SSL/超时);`api.nuget.org` 对 .NET 客户端 TLS 被掐(仓库签名端点 `repository-signatures/...` 必炸 NU1301)。`nuget.azure.cn` 镜像可 curl 可达,但它的服务索引把 `repositorySignatures` 指回 `api.nuget.org`,导致 NuGet 客户端还原必失败。
- **解决办法**:完全离线。`NuGet.config` 只保留一个本地平铺源 `D:\lg\else\mod\.tools\nuget-feed\`(7 个 nupkg:Godot.NET.Sdk/SourceGenerators/GodotSharp/GodotSharpEditor 4.5.1 + BaseLib 3.4.0 + ModAnalyzers 0.1.9 + Krafs.Publicizer 2.3.0)。本地源没有仓库签名概念,还原零网络。
- `Godot.NET.Sdk` 不来自 NuGet 官方源,而是 Godot mono 编辑器自带:`GodotSharp\Tools\nupkgs\`。

### 构建 / 部署工作流

```bash
# 构建(编译用游戏自带 dll,不需要 Godot 编辑器)
cd D:/lg/else/mod/src/PartSmith
D:/lg/else/mod/.tools/dotnet/dotnet.exe build PartSmith.csproj -c Debug \
  -p:Sts2Path="D:/Steam/steamapps/common/Slay the Spire 2"
# 产物:dist/PartSmith/{PartSmith.dll, PartSmith.json, PartSmith.pdb}
```

- 构建**不自动部署到游戏目录**(按用户偏好,见 csproj 的 `CopyToModsFolderOnBuild` 目标,改为拷贝到 `dist/`)。
- 显式部署:`bash tools/deploy-to-game.sh`(拷 `dist/PartSmith/*` → 游戏 `mods/PartSmith/`)。VSCode 任务 `Ctrl+Shift+B` 构建;`Deploy to game (manual)` 一键部署。
- **首次运行时依赖**:把 `dist/BaseLib/{BaseLib.dll,BaseLib.json,BaseLib.pck}` 拷到游戏 `mods/BaseLib/`(一次性)。游戏内 `Settings → Mod Settings` 开启。

### 待办里程碑

1. 用 `ConstructedCardModel` 做"局内按参数生成新卡并进牌组"的原型(验证机制成立)。
2. 实现零件池(资源零件 × 效果零件)。
3. 改卡牌奖励 UI(两段式选择)。
