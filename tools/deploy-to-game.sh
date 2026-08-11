#!/bin/bash
# 手动部署:把构建产物(dist/PartSmith)复制到游戏 mods 目录。
# 构建不会自动部署(见 csproj 的 CopyToModsFolderOnBuild 目标),这一步显式执行。
set -e

GAME="D:/Steam/steamapps/common/Slay the Spire 2"
MOD="PartSmith"
SRC="D:/lg/else/mod/src/PartSmith/dist/$MOD"
DST="$GAME/mods/$MOD"

if [ ! -f "$SRC/$MOD.dll" ] || [ ! -f "$SRC/$MOD.json" ]; then
  echo "未找到构建产物,请先构建(在 VSCode 里按 Ctrl+Shift+B,或运行 build 任务)。"
  exit 1
fi

mkdir -p "$DST"
cp "$SRC/$MOD.dll" "$SRC/$MOD.json" "$SRC/$MOD.pdb" "$DST/"
echo "已部署到: $DST"
