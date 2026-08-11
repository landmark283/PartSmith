# Bridge 驱动游戏内测卡流程(PartSmith)

> 2026-08-10 起用。用 sts2-bridge(端口 27100)+ `parttest` 自测命令在游戏里直接验证「显示是否正确 / 打出是否有预期效果」,不依赖人工肉测。
> 配套驱动脚本:`tools/verify_in_game.py`(场景化回归,`python tools/verify_in_game.py --scenario <name|all>`)。

---

## 1. 基础设施

| 组件 | 说明 |
|---|---|
| **sts2-bridge** | 游戏内 mod,HTTP 服务器,端口 27100;token 在 `%APPDATA%\SlayTheSpire2\bridge\session.json`(`token` 字段)。端点:`GET /health`(免鉴权)、`GET /state`、`POST /action`、`POST /console`。 |
| **`parttest`** | mod 自测控制台命令(`PartSmithCode/DevConsole/PartSelfTestCommand.cs`):`make` 现造拼卡塞手牌 / `info <handIdx>` 读真实渲染 / `room` 直达测试战斗 / `maxhp` 敌人顶到 int 上限 / `encounters` 列遭遇 id。容量绕过(CanSplice 校验跳过),HowlFromBeyond 这种 16 点也能造。 |
| **`tools/verify_in_game.py`** | Python(纯 stdlib)驱动:构建部署 → 启动等待 → 确保 run → 跑场景 → 断言汇总,输出 PASS/FAIL。 |

---

## 2. 前置准备(改动后必做)

1. **构建**:`dotnet build PartSmith.csproj --force`(要 0 错 0 警)。
2. **部署**:
   - 只改 C#:`tools/deploy-to-game.sh` 拷 dll/json/pdb 到 `游戏目录/mods/PartSmith/`。
   - **改本地化必须重打 pck**:cards.json 在 `src/PartSmith/PartSmith/localization/{eng,zhs}/`,游戏从 `res://{modId}/localization/` 加载;用 Godot `--headless --export-pack "BasicExport"` 导出后再拷 pck。
3. **部署前游戏必须关闭**(dll 被占用报 Device or resource busy)。
4. **改 bridge 源码要自己重建**:`.tools/sts2-mcp/mods/sts2-bridge/`,`dotnet build -p:Sts2Dir=游戏目录 -p:Sts2SkipDeploy=true`,手拷 dll 到游戏 mods。⚠ async 方法里不能 `return Task.FromResult(anon)`(响应会被包成 Task 信封),直接 return。
5. **启动游戏**:`D:/Steam/steam.exe -applaunch 2868840`(有网络问题时**用户手动启动最稳**,Claude 直跑 exe 不可靠);轮询 `GET /health` ≤240s。

---

## 3. 手动驱动流程(逐步)

1. **连 bridge**:读 session.json 拿 base_url + token;`connect_bridge(timeout=...)`。
2. **确保有 run**:无 run → 按 `/state.available_actions` 动态点 `main_menu:new_game` → `run_mode:standard` → `character_select:<idx>`(优先 BigWarrior)→ `embark`(禁止硬编码 action id;角色与卡测无关)。run 激活后**等 ~10s** 让地图/事件房间稳定。
3. **进测试房间**:`/console {"command":"parttest room"}`(默认 KNIGHTS_ELITE 3 敌;可带 encounterId)。轮询 /state 直到 `screen=="COMBAT"` 且 `combat.in_progress`。
   - **每次要干净状态就重新 `parttest room`**(先退出当前房间再进新战斗 → 干净回合历史 + 无残留 Power)。
4. **控状态**:`parttest maxhp`(敌人 999999999≈int 上限)、`energy 20`、`block 999999 0`(保护玩家)。
5. **现造拼卡**:`parttest make PARTSMITH-XXX_FRAGMENT[,PARTSMITH-YYY_FRAGMENT...]`(宿主默认 Scrap 0 费;可显式 `make <costCardId> <effectId[,effectId...]>`)。返回渲染后的 Title + Description。
6. **读真实渲染**:`parttest info <handIdx>`(/state 的卡描述是基础值,不反映升级/动态修正)。
7. **打牌 / 结束回合**:`/action`(action_id 从 available_actions 现读、目标从 `combat.target_index_map` 取、带 `expected_state_version` 防陈旧);**`end_turn` 是 action,不是 console 命令**。
8. **断言**:读 /state 的 `combat.enemy_creatures[].current_hp`,算 Δ。

---

## 4. 常用命令速查

| 命令 | 作用 |
|---|---|
| `parttest make <PARTSMITH-ID>[,<PARTSMITH-ID>...]` | 现造拼卡塞手牌(**id 要全名前缀**) |
| `parttest make <costCardId> <effectId[,effectId...]>` | 显式指定宿主费用卡 |
| `parttest info <handIdx>` | 读手牌某卡 title/desc/type/target/keywords(真实渲染) |
| `parttest room [encounterId]` | 直达测试战斗(可重置干净状态) |
| `parttest maxhp` | 敌人 HP 顶到 int 上限 |
| `parttest encounters` | 列合法遭遇 id |
| `parttest library [pool]` | **百科大全(图鉴)自检**:列出该池在图鉴里应显示的卡及可见状态 `LOCKED`/`NOT_SEEN`/`VISIBLE`(默认 effect;可选 cost/hunter_effect/hunter_cost/all)。排查"某张卡图鉴里不显示"先跑它 |
| `energy 20` / `block 999999 0` | 控玩家能量 / 给无敌格挡 |
| `upgrade <handIdx>` | 升级手牌某卡 |

---

## 5. 注意事项(踩过的坑)

- **`parttest make` 的 effectId 必须带全 `PARTSMITH-` 前缀**(`Make` 用 `Id.Entry == id` 精确匹配,短名 `STOMP_FRAGMENT` 会报 "not found")。
- **`end_turn` 是 /action**:用 console 发会 "The command 'end_turn' does not exist"。
- **/state 的卡描述是基础值**:不反映升级/力量/易伤修正,要看真实渲染用 `parttest info`。
- **/action 会自动完成单选项选牌界面**(如头槌只剩一张可选时),不会停住等人工。
- **测伤害要 Δ = ΔHP + Δ格挡**(敌人有格挡先吃格挡)。
- **玩家不保护会被打死**(game over),测复播/自动打前先 `block 999999 0`。
- **桥驱动要慢**:每条命令后轮询 /state 稳定再发下一条;房间切换是异步的,要等 `combat.in_progress`。
- **自计数类卡(践踏)测 0 攻击 Δ0 必须在干净回合**:同回合先打过的攻击牌(含之前打过的践踏)会算进「本回合已打出攻击」。**强制 `parttest room` 最干净**。
- **随机目标类效果(惊逃 StampedePower 回合末自动打)断言"至少一个敌人掉血"(合计 Δ>0)**:目标随机,单敌断言 Δ>0 会有假 FAIL。
- **战斗中途 parttest make 加卡**:直接在**当前战斗**的手牌里(combat scope),可当回合使用;与 partsplice 只改 run 牌组(下场战斗才生效)不同。

---

## 6. 典型场景清单(verify_in_game.py)

通过修改verify_in_game.py 可以验证不同的卡片。不过，脚本操作可能触发意外的bug，建议只在脚本中写卡片验证有关的操作，选择角色，开始游戏，进入房间的流程应该在脚本外完成。

| 场景 | 验证点 | 断言口径 |
|---|---|---|
| `armaments_upgrade` | 军械升级描述切换 | 描述含 "Upgrade all cards in your hand." / 「升级你的所有手牌。」 |
| `stampede_plain` / `stampede_upgraded` | 践踏 0 / 1 / 升级+1 攻击 Δ | 每敌 Δ0 / Δ4 / Δ8(干净回合) |
| `headbutt` | 弃牌堆 → 抽牌堆顶 | 选牌界面 + 弃牌那张到抽牌堆顶 |
| `howl_from_beyond` | 消耗后回合末自动复播 | 打出 Δ18 → 结束回合 → 再 Δ18(玩家先 block) |
| `card_library` | **百科大全能找到卡且可见** | `parttest library effect` 输出含 践踏/惊逃 且状态 VISIBLE(数据层;UI 层默认选中费用池,效果卡要自己点绿色按钮) |
| `verify_rename.py`(临时) | 践踏/惊逃标题 + 效果 | 标题断言 + 践踏 0 攻击 Δ0 + 惊逃自动打「至少一敌 Δ>0」 |

---

## 7. 环境约束 / 常用路径

- **游戏启动**:用户手动最稳(`steam -applaunch` 遇到 steam 网络问题会失败,直跑 exe 有 DRM 检查)。
- **令牌 / bridge 状态**:`%APPDATA%\SlayTheSpire2\bridge\session.json`。
- **游戏日志**:`%APPDATA%\SlayTheSpire2\logs\godot*.log`(user:// 被游戏重定向到 Roaming/SlayTheSpire2,排查异常用)。
- **部署产物**:游戏 mods 目录 `D:\Steam\steamapps\common\Slay the Spire 2\mods\PartSmith\`(dll/json/pdb/pck)。
