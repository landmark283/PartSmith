#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
PartSmith 自动化游戏内验证驱动。

通过 sts2-mcp bridge(端口 27100,session 在 %APPDATA%/SlayTheSpire2/bridge/session.json)
在真游戏里自动验证卡片显示与效果。每次改卡后跑一遍 = 回归。

用法:
  python tools/verify_in_game.py                # 构建+部署+启动+全部场景
  python tools/verify_in_game.py --scenario armaments_upgrade
  python tools/verify_in_game.py --no-deploy    # 跳过构建/部署(游戏已在跑新 dll)
  python tools/verify_in_game.py --no-launch    # 不自动启动游戏(需已手动开着)
  python tools/verify_in_game.py --close        # 测完关游戏(默认保持打开)
  python tools/verify_in_game.py --force        # 允许自动关闭正在运行的游戏(部署前)

场景:
  armaments_upgrade  升级军械拼卡描述 → 含「升级你的所有手牌。」
  stampede           践踏:0攻击基础0 / 打1张攻击+4 / 升级后基础4+1攻击=8
  headbutt           头槌:弹出选牌界面 → 弃牌堆那张放到抽牌堆顶
  howl_from_beyond   幽冥嚎叫:打18伤害进消耗,结束回合自动复播再18
  card_library       百科大全(图鉴):效果池能找到践踏/惊逃且 VISIBLE(数据层自检)
"""
import argparse
import json
import os
import shutil
import subprocess
import sys
import time
import traceback
import urllib.request
import urllib.error

if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8", errors="replace")
    sys.stderr.reconfigure(encoding="utf-8", errors="replace")

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))  # D:\lg\else\mod
SRC = os.path.join(ROOT, "src", "PartSmith")
PROJECT = SRC
CSPROJ = os.path.join(SRC, "PartSmith.csproj")
DIST = os.path.join(SRC, "dist", "PartSmith")
GAME = r"D:\Steam\steamapps\common\Slay the Spire 2"
GAME_MODS = os.path.join(GAME, "mods", "PartSmith")
STEAM = r"D:\Steam\steam.exe"
APP_ID = "2868840"
SESSION_PATH = os.path.join(os.environ.get("APPDATA", ""), "SlayTheSpire2", "bridge", "session.json")
DOTNET = os.path.join(ROOT, ".tools", "dotnet", "dotnet.exe")
GODOT = r"E:\Godot\Godot_v4.5.1-stable_mono_win64\Godot_v4.5.1-stable_mono_win64_console.exe"
RESULTS_LOG = os.path.join(ROOT, "tools", ".verify-results.log")

POST_RUN_SCREENS = {"MAP", "COMBAT", "REST_SITE", "SHOP", "REWARDS", "CARD_REWARD_SELECTION",
                    "CARD_SELECTION", "DECK_UPGRADE_SELECTION", "EVENT", "TREASURE", "EVENT_CRYSTAL_SPHERE"}


# ---------------------------------------------------------------- bridge client

class Bridge:
    def __init__(self, base_url, token):
        self.base = base_url.rstrip("/")
        self.token = token

    def _req(self, method, path, body=None, auth=True, timeout=30):
        url = self.base + path
        data = json.dumps(body).encode("utf-8") if body is not None else None
        req = urllib.request.Request(url, data=data, method=method)
        if body is not None:
            req.add_header("Content-Type", "application/json")
        if auth:
            req.add_header("Authorization", "Bearer " + self.token)
        try:
            with urllib.request.urlopen(req, timeout=timeout) as r:
                return json.loads(r.read().decode("utf-8"))
        except urllib.error.HTTPError as e:
            raw = e.read().decode("utf-8", "replace")
            try:
                return json.loads(raw)
            except Exception:
                return {"ok": False, "error": f"HTTP {e.code}", "message": raw[:500]}

    def health(self):
        return self._req("GET", "/health", auth=False, timeout=3)

    def state(self):
        st = self._req("GET", "/state")
        if not st.get("ok"):
            raise RuntimeError("GET /state failed: " + str(st)[:400])
        return st

    def action(self, action_id, wait_after_ms=0):
        r = self._req("POST", "/action", {"action_id": action_id, "wait_after_ms": wait_after_ms})
        if not r.get("ok"):
            raise RuntimeError(f"action '{action_id}' failed: {r.get('error')} {str(r.get('message'))[:300]}")
        return r

    def console(self, command):
        r = self._req("POST", "/console", {"command": command})
        if not r.get("ok"):
            raise RuntimeError(f"console '{command}' failed: {str(r.get('output') or r.get('message'))[:300]}")
        return r

    def wait_until(self, pred, timeout=30.0, interval=0.4, label="state"):
        deadline = time.time() + timeout
        last = None
        while time.time() < deadline:
            last = self.state()
            if pred(last):
                return last
            time.sleep(interval)
        raise TimeoutError(f"Timed out waiting for {label}")


# ---------------------------------------------------------------- state helpers

def hand_cards(st):
    players = st.get("players") or []
    if not players:
        return []
    return players[0].get("combat", {}).get("hand", {}).get("cards", []) or []


def pile_cards(st, pile_name):
    players = st.get("players") or []
    if not players:
        return []
    return players[0].get("combat", {}).get(pile_name, {}).get("cards", []) or []


def enemies(st):
    return st.get("combat", {}).get("enemy_creatures") or []


def find_hand_idx(st, *titles):
    cards = hand_cards(st)
    for i, c in enumerate(cards):
        t = (c.get("title") or "").lower()
        if any(tk.lower() in t for tk in titles):
            return i
    return None


def run_active(st):
    return bool((st.get("run") or {}).get("has_run"))


# ---------------------------------------------------------------- deploy / launch

def is_game_running():
    try:
        r = subprocess.run(["tasklist", "/FI", "IMAGENAME eq SlayTheSpire2.exe"],
                           capture_output=True, text=True, errors="replace", timeout=20)
        return "SlayTheSpire2.exe" in r.stdout
    except Exception:
        return True


def kill_game():
    print("[deploy] closing game...")
    subprocess.run(["taskkill", "/F", "/IM", "SlayTheSpire2.exe"],
                   capture_output=True, text=True, errors="replace")
    for _ in range(30):
        if not is_game_running():
            return
        time.sleep(1)
    print("[deploy] WARN: game process still present after taskkill")


def run_build():
    print("[build] dotnet build -c Debug ...")
    r = subprocess.run([DOTNET, "build", CSPROJ, "-c", "Debug"], cwd=ROOT,
                       capture_output=True, text=True, errors="replace", timeout=600)
    tail = (r.stdout or "")[-2500:] + (r.stderr or "")[-1500:]
    print(tail)
    if r.returncode != 0:
        raise RuntimeError("dotnet build failed")


def export_pck():
    pck = os.path.join(DIST, "PartSmith.pck")
    print("[build] exporting pck ...")
    r = subprocess.run([GODOT, "--headless", "--path", PROJECT, "--export-pack", "BasicExport", pck],
                       capture_output=True, text=True, errors="replace", timeout=600)
    if not os.path.exists(pck):
        raise RuntimeError("pck export failed: " + (r.stdout or "")[-800:] + (r.stderr or "")[-800:])
    print(f"[build] pck ok ({os.path.getsize(pck)} bytes)")


def deploy():
    print("[deploy] copying dll/json/pdb/pck to game mods ...")
    os.makedirs(GAME_MODS, exist_ok=True)
    for f in ("PartSmith.dll", "PartSmith.json", "PartSmith.pdb", "PartSmith.pck"):
        shutil.copy2(os.path.join(DIST, f), os.path.join(GAME_MODS, f))
    print("[deploy] done")


def connect_bridge(timeout=90):
    deadline = time.time() + timeout
    while time.time() < deadline:
        try:
            with open(SESSION_PATH, "r", encoding="utf-8") as fh:
                s = json.load(fh)
            base, token = s["base_url"], s["token"]
        except Exception:
            time.sleep(2)
            continue
        try:
            b = Bridge(base, token)
            h = b.health()
            if h.get("ok"):
                b.state()  # verify auth works
                print(f"[bridge] connected {b.base}")
                return b
        except Exception:
            pass
        time.sleep(2)
    raise RuntimeError("Bridge not reachable")


# ---------------------------------------------------------------- run start

def pick_action(acts, pred):
    return next((a for a in acts if pred(a)), None)


def start_run(bridge):
    # 等游戏完成启动(屏别停在 UNKNOWN)。
    bridge.wait_until(lambda s: (s.get("screen") or "UNKNOWN") != "UNKNOWN",
                      timeout=90, label="menu ready")
    for _ in range(20):
        st = bridge.state()
        screen = st.get("screen") or "UNKNOWN"
        run = st.get("run") or {}
        acts = st.get("available_actions") or []

        # 放弃确认弹窗(在 MAIN_MENU 之上)。
        if any(str(a.get("action_id", "")).startswith("main_menu:abandon_confirm:")
               or a.get("action_id") == "main_menu:confirm_abandon_run"
               for a in acts):
            a = pick_action(acts, lambda x: x.get("action_id") == "main_menu:confirm_abandon_run"
                            or x.get("action_id", "").startswith("main_menu:abandon_confirm:"))
            print(f"[run] confirm abandon -> {a['action_id']}")
            bridge.action(a["action_id"])
            time.sleep(1.0)
            continue

        if screen == "MAIN_MENU":
            has_save = any(a.get("action_id") in ("main_menu:abandon_current_game", "main_menu:continue")
                           for a in acts)
            if has_save:
                # 有存档:先放弃,单机(新游戏)按钮才出现。
                a = pick_action(acts, lambda x: x.get("action_id") == "main_menu:abandon_current_game"
                                or x.get("menu_action") == "abandon_current_game")
                if a is None:
                    raise RuntimeError(f"MAIN_MENU(save): no abandon action. actions={[x.get('action_id') for x in acts][:14]}")
                print("[run] main_menu -> abandon_current_game")
                bridge.action(a["action_id"])
            else:
                a = pick_action(acts, lambda x: x.get("action_id") == "main_menu:singleplayer"
                                or x.get("menu_action") == "singleplayer")
                if a is None:
                    raise RuntimeError(f"MAIN_MENU(fresh): no singleplayer action. actions={[x.get('action_id') for x in acts][:14]}")
                print("[run] main_menu -> singleplayer")
                bridge.action(a["action_id"])
        elif screen == "RUN_MODE_SELECTION":
            a = pick_action(acts, lambda x: "standard" in str(x.get("action_id", ""))
                            or "standard" in str(x.get("label", "")).lower()
                            or "标准" in str(x.get("label", ""))
                            or "普通" in str(x.get("label", "")))
            if a is None:
                a = pick_action(acts, lambda x: str(x.get("action_id", "")).startswith("run_mode:"))
            if a is None:
                raise RuntimeError(f"RUN_MODE_SELECTION: no run_mode action. actions={[x.get('action_id') for x in acts][:12]}")
            print("[run] run_mode -> standard")
            bridge.action(a["action_id"])
        elif screen == "CHARACTER_SELECT":
            aid = pick_character_action(st)
            print(f"[run] character_select -> {aid}")
            bridge.action(aid)
        elif run_active(st):
            print(f"[run] fresh run active (screen={screen})")
            return st
        else:
            raise RuntimeError(f"Unexpected screen {screen} — return to main menu or re-run with deploy. "
                               f"actions={[x.get('action_id') for x in acts][:8]}")
        time.sleep(0.6)
    raise RuntimeError("Timed out starting a run")


def pick_character_action(st):
    acts = st.get("available_actions") or []
    cs = st.get("character_selection") or {}
    if cs.get("selected_character") and any(a.get("action_id") == "embark" for a in acts):
        return "embark"
    cs_acts = [a for a in acts
               if a.get("kind") == "character_select"
               and str(a.get("action_id", "")).startswith("character_select:")]
    if not cs_acts:
        raise RuntimeError("CHARACTER_SELECT: no character_select action available")
    for a in cs_acts:
        ch = json.dumps(a.get("character") or {}, ensure_ascii=False)
        if "BIG_WARRIOR" in ch.upper() or "大战士" in ch:
            return a["action_id"]
    return cs_acts[0]["action_id"]


# ---------------------------------------------------------------- combat helpers

def start_combat(bridge, encounter):
    bridge.console(f"fight {encounter}")
    bridge.wait_until(lambda st: st.get("screen") == "COMBAT" and st.get("combat", {}).get("in_progress") is True,
                      timeout=90, label="COMBAT start")
    bridge.console("energy 20")
    bridge.wait_until(lambda st: bool(hand_cards(st)), timeout=20, label="hand dealt")


def play_card(bridge, idx, target_combat_id=None):
    st = bridge.state()
    acts = st.get("available_actions") or []
    cands = [a for a in acts if a.get("kind") == "play_card" and a.get("hand_index") == idx]
    if not cands:
        have = [a.get("action_id") for a in acts if a.get("kind") == "play_card"]
        raise RuntimeError(f"No play_card action for hand idx {idx}. play actions: {have}")
    if target_combat_id is not None:
        cands = [a for a in cands if a.get("target_combat_id") == target_combat_id]
        if not cands:
            raise RuntimeError(f"No play_card target {target_combat_id} for idx {idx}")
    elif len(cands) > 1:
        no_tgt = [a for a in cands if not a.get("target_action_suffix")]
        if no_tgt:
            cands = no_tgt
    bridge.action(cands[0]["action_id"])


def end_turn(bridge):
    st = bridge.state()
    acts = st.get("available_actions") or []
    a = pick_action(acts, lambda x: x.get("action_id") == "end_turn")
    if a is None:
        raise RuntimeError("No end_turn action available")
    bridge.action("end_turn")


def enemy_hp(st):
    es = enemies(st)
    return {e["combat_id"]: e["current_hp"] for e in es}


# ---------------------------------------------------------------- scenarios

def scenario_armaments(bridge, res):
    start_combat(bridge, "FROG_KNIGHT_NORMAL")
    bridge.console("parttest make PARTSMITH-ARMAMENTS_FRAGMENT")
    st = bridge.wait_until(lambda s: find_hand_idx(s, "军械", "Armament") is not None, timeout=20, label="armaments in hand")
    idx = find_hand_idx(st, "军械", "Armament")
    desc0 = hand_cards(st)[idx].get("description") or ""
    res.record("armaments.base", "升级前:基础分支(无「所有」/ Upgrade all)",
               "所有" not in desc0 and "Upgrade all" not in desc0, desc0[:140])
    bridge.console(f"upgrade {idx}")
    st = bridge.wait_until(lambda s: find_hand_idx(s, "军械", "Armament") is not None, timeout=10)
    idx = find_hand_idx(st, "军械", "Armament")
    desc1 = hand_cards(st)[idx].get("description") or ""
    ok = ("所有" in desc1) or ("Upgrade all" in desc1)
    res.record("armaments.upgraded", "升级后:含「升级你的所有手牌。」/ Upgrade all cards in your hand.", ok, desc1[:160])


def scenario_stampede(bridge, res):
    # Part 1: 未打攻击 → 基础 0 伤害
    start_combat(bridge, "CULTISTS_NORMAL")
    bridge.console("parttest make PARTSMITH-STOMP_FRAGMENT")
    st = bridge.wait_until(lambda s: find_hand_idx(s, "践踏", "Stomp") is not None, timeout=20, label="stampede in hand")
    idx = find_hand_idx(st, "践踏", "Stomp")
    hp0 = enemy_hp(st)
    play_card(bridge, idx)
    st = bridge.state()
    hp1 = enemy_hp(st)
    for cid in hp0:
        res.record(f"stampede.zero.{cid}", "0攻击时每敌 Δ0", hp1.get(cid, 0) == hp0[cid], f"hp {hp0[cid]}->{hp1.get(cid)}")

    # Part 2: 打1张攻击 → 基础0 + 4×1 = 4
    start_combat(bridge, "CULTISTS_NORMAL")
    bridge.console("parttest make PARTSMITH-STOMP_FRAGMENT")
    st = bridge.wait_until(lambda s: find_hand_idx(s, "践踏", "Stomp") is not None, timeout=20)
    idx = find_hand_idx(st, "践踏", "Stomp")
    bridge.console("card STRIKE_IRONCLAD Hand")
    st = bridge.state()
    idx_strike = find_hand_idx(st, "打击", "Strike")
    if idx_strike is None:
        raise RuntimeError("No strike found in hand after 'card STRIKE_IRONCLAD Hand'")
    hp0 = enemy_hp(st)
    play_card(bridge, idx_strike)
    play_card(bridge, idx)
    st = bridge.state()
    hp1 = enemy_hp(st)
    for cid in hp0:
        d = hp0[cid] - hp1.get(cid, 0)
        res.record(f"stampede.one_attack.{cid}", "1攻击时每敌 Δ4", d == 4, f"hp {hp0[cid]}->{hp1.get(cid)} (Δ{d})")

    # Part 3: 升级 → 基础4 + 4×1 = 8
    start_combat(bridge, "CULTISTS_NORMAL")
    bridge.console("parttest make PARTSMITH-STOMP_FRAGMENT")
    st = bridge.wait_until(lambda s: find_hand_idx(s, "践踏", "Stomp") is not None, timeout=20)
    idx = find_hand_idx(st, "践踏", "Stomp")
    bridge.console(f"upgrade {idx}")
    bridge.console("card STRIKE_IRONCLAD Hand")
    st = bridge.state()
    idx_strike = find_hand_idx(st, "打击", "Strike")
    if idx_strike is None:
        raise RuntimeError("No strike found in hand after 'card STRIKE_IRONCLAD Hand'")
    hp0 = enemy_hp(st)
    play_card(bridge, idx_strike)
    play_card(bridge, idx)
    st = bridge.state()
    hp1 = enemy_hp(st)
    for cid in hp0:
        d = hp0[cid] - hp1.get(cid, 0)
        res.record(f"stampede.upgraded.{cid}", "升级+1攻击时每敌 Δ8", d == 8, f"hp {hp0[cid]}->{hp1.get(cid)} (Δ{d})")


def scenario_headbutt(bridge, res):
    start_combat(bridge, "FROG_KNIGHT_NORMAL")
    bridge.console("card STRIKE_IRONCLAD Discard")
    st = bridge.wait_until(lambda s: len(pile_cards(s, "discard_pile")) >= 1, timeout=20, label="card in discard")
    discard_title = pile_cards(st, "discard_pile")[0].get("title")
    bridge.console("parttest make PARTSMITH-HEADBUTT_FRAGMENT")
    st = bridge.wait_until(lambda s: find_hand_idx(s, "头槌", "Headbutt") is not None, timeout=20, label="headbutt in hand")
    idx = find_hand_idx(st, "头槌", "Headbutt")
    tgt = enemies(st)[0]["combat_id"] if enemies(st) else None
    play_card(bridge, idx, target_combat_id=tgt)
    st = bridge.wait_until(lambda s: any(a.get("kind") == "card_selection" and a.get("selection_action") == "select"
                                         for a in s.get("available_actions") or []),
                           timeout=40, label="headbutt selection screen")
    res.record("headbutt.selection", "打头槌后弹出选牌界面", True, "card_selection:select available")
    sel = [a for a in st.get("available_actions") or []
           if a.get("kind") == "card_selection" and a.get("selection_action") == "select"]
    opt = next((a for a in sel if (a.get("card") or {}).get("title") == discard_title), sel[0])
    bridge.action(opt["action_id"])
    st = bridge.state()
    conf = pick_action(st.get("available_actions") or [], lambda x: x.get("action_id") == "card_selection:confirm")
    if conf:
        bridge.action("card_selection:confirm")
    st = bridge.wait_until(lambda s: (lambda d: len(d) >= 1 and (d[0].get("title") or "") == discard_title)(
                              pile_cards(s, "draw_pile")), timeout=20, label="card on top of draw pile")
    top = pile_cards(st, "draw_pile")[0].get("title")
    res.record("headbutt.draw_top", "弃牌堆那张放到抽牌堆顶", top == discard_title, f"top={top} expected={discard_title}")


def scenario_howl(bridge, res):
    start_combat(bridge, "FROG_KNIGHT_NORMAL")
    bridge.console("parttest make PARTSMITH-HOWL_FROM_BEYOND_FRAGMENT")
    st = bridge.wait_until(lambda s: find_hand_idx(s, "幽冥嚎叫", "Howl From Beyond", "Howl From") is not None,
                           timeout=20, label="howl in hand")
    idx = find_hand_idx(st, "幽冥嚎叫", "Howl From Beyond", "Howl From")
    hp0 = enemy_hp(st)
    play_card(bridge, idx)  # AoE, no target
    st = bridge.state()
    hp1 = enemy_hp(st)
    cid = list(hp0)[0]
    d1 = hp0[cid] - hp1.get(cid, 0)
    res.record("howl.first", "首击 Δ18", d1 == min(18, hp0[cid]), f"hp {hp0[cid]}->{hp1.get(cid)} (Δ{d1})")
    ex = pile_cards(st, "exhaust_pile")
    in_ex = any(("Howl" in (c.get("title") or "")) or ("嚎叫" in (c.get("title") or "")) for c in ex)
    res.record("howl.exhaust", "打出后进消耗堆", in_ex, f"exhaust={[c.get('title') for c in ex]}")
    if hp1.get(cid, 0) == 0:
        res.record("howl.replay", "复播验证", False, "敌人首击已死,无法观察复播(SKIP)", skip=True)
        return
    bridge.console("heal 999")
    bridge.console("block 999")
    end_turn(bridge)
    try:
        st = bridge.wait_until(lambda s: (enemies(s) and enemies(s)[0]["current_hp"] < hp1[cid]) or s.get("screen") != "COMBAT",
                               timeout=60, label="replay damage")
    except TimeoutError:
        res.record("howl.replay", "结束回合后敌人再次掉血", False, "60s 内未见第二次掉血")
        return
    if st.get("screen") != "COMBAT":
        # 战斗结束(敌人被复播打死) → hp 归 0,掉血 = hp1
        d2 = hp1[cid]
        res.record("howl.replay", "复播把敌人打死(Δ=剩余HP)", d2 == min(18, hp1[cid]), f"combat ended, hp {hp1[cid]}->0")
    else:
        hp2 = enemies(st)[0]["current_hp"]
        d2 = hp1[cid] - hp2
        res.record("howl.replay", "结束回合后复播再 Δ18", d2 == min(18, hp1[cid]), f"hp {hp1[cid]}->{hp2} (Δ{d2})")


def scenario_card_library(bridge, res):
    """百科大全(卡牌图鉴):效果池里能找到践踏/惊逃且可见(VISIBLE)。数据层自检,不需战斗。"""
    r = bridge.console("parttest library effect")
    out = r.get("output") or ""

    header = next((l for l in out.splitlines() if l.startswith("[PartSmithEffectCardPool]")), None)
    res.record("library.effectpool", "效果池过滤存在且匹配>0",
               header is not None and "matching=" in (header or ""), header or "POOL NOT FOUND")

    for entry, zh in (("PARTSMITH-STOMP_FRAGMENT", "践踏"), ("PARTSMITH-STAMPEDE_FRAGMENT", "惊逃")):
        line = next((l for l in out.splitlines() if l.strip().startswith(entry)), None)
        ok = line is not None and "VISIBLE" in line and zh in line
        res.record(f"library.{entry}", f"百科大全效果池含 {zh} 且 VISIBLE", ok, line or "NOT FOUND")


SCENARIOS = {
    "armaments_upgrade": scenario_armaments,
    "stampede": scenario_stampede,
    "headbutt": scenario_headbutt,
    "howl_from_beyond": scenario_howl,
    "card_library": scenario_card_library,
}


# ---------------------------------------------------------------- results

class Results:
    def __init__(self):
        self.checks = []

    def record(self, name, what, passed, detail="", skip=False):
        tag = "SKIP" if skip else ("PASS" if passed else "FAIL")
        self.checks.append({"name": name, "what": what, "passed": bool(passed), "skip": skip, "detail": detail})
        print(f"  [{tag}] {name}: {what}" + (f"  | {detail}" if detail else ""))
        sys.stdout.flush()

    def summary(self):
        passed = sum(1 for c in self.checks if c["passed"])
        skipped = sum(1 for c in self.checks if c["skip"])
        failed = sum(1 for c in self.checks if not c["passed"] and not c["skip"])
        total = len(self.checks)
        print(f"\n===== {passed} passed, {failed} failed, {skipped} skipped / {total} checks =====")
        return failed == 0


# ---------------------------------------------------------------- main

def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--scenario", default="all", help="comma list of scenario names or 'all'")
    ap.add_argument("--no-deploy", action="store_true", help="skip build/export/deploy")
    ap.add_argument("--no-launch", action="store_true", help="do not auto-launch the game")
    ap.add_argument("--close", action="store_true", help="close the game after tests")
    ap.add_argument("--force", action="store_true", help="allow auto-closing a running game before deploy")
    args = ap.parse_args()

    names = sorted(SCENARIOS) if args.scenario == "all" else [s.strip() for s in args.scenario.split(",")]
    for n in names:
        if n not in SCENARIOS:
            print(f"Unknown scenario '{n}'. Available: {', '.join(sorted(SCENARIOS))}")
            sys.exit(2)

    if not args.no_deploy:
        if is_game_running():
            if not args.force:
                print("Game is running. Close it (or pass --force). Aborting before deploy.")
                sys.exit(1)
            kill_game()
        run_build()
        export_pck()
        deploy()

    bridge = None
    try:
        bridge = connect_bridge(timeout=15)
    except Exception:
        if args.no_launch:
            raise RuntimeError("Game not running / bridge not reachable (and --no-launch).")
        print("[launch] starting game via steam...")
        subprocess.Popen([STEAM, "-applaunch", APP_ID])
        bridge = connect_bridge(timeout=240)

    try:
        start_run(bridge)
    except Exception as e:
        print(f"[run] failed to start run: {e}")
        print("       You can launch the game manually and enter a run, then re-run with --no-deploy --no-launch.")
        sys.exit(1)

    res = Results()
    for name in names:
        print(f"\n=== scenario: {name} ===")
        try:
            SCENARIOS[name](bridge, res)
        except Exception as e:
            res.record(name, "scenario threw", False, f"{type(e).__name__}: {e}")
            print(traceback.format_exc())

    ok = res.summary()
    with open(RESULTS_LOG, "w", encoding="utf-8") as fh:
        json.dump({"ok": ok, "checks": res.checks, "time": time.strftime("%Y-%m-%d %H:%M:%S")},
                  fh, ensure_ascii=False, indent=2)
    print(f"results -> {RESULTS_LOG}")

    if args.close:
        kill_game()
    sys.exit(0 if ok else 1)


if __name__ == "__main__":
    main()
