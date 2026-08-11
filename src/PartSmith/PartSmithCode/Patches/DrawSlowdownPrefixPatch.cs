using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using PartSmith.PartSmithCode.Powers;

namespace PartSmith.PartSmithCode.Patches;

/// <summary>
/// 动作缓慢(Slowdown)对"卡片效果抽牌"生效:走 SlowdownPower.Consume 按抽牌数消耗层数并减 count。
/// 回合开始抽牌(CombatManager 已把 ModifyHandDraw 结果传进来,fromHandDraw=true)由
/// SlowdownPower.ModifyHandDraw 消耗过 → 这里跳过,防双扣。所有 Draw 重载都汇聚到
/// Draw(choiceContext, count, player, fromHandDraw),prefix 挂在 4 参版本即可全捕获。
/// </summary>
[HarmonyPatch(
    typeof(CardPileCmd),
    nameof(CardPileCmd.Draw),
    new[] { typeof(PlayerChoiceContext), typeof(decimal), typeof(Player), typeof(bool) })]
internal static class DrawSlowdownPrefixPatch
{
    private static void Prefix(Player player, ref decimal count, bool fromHandDraw)
    {
        if (fromHandDraw)
        {
            return; // 回合开始抽牌已由 SlowdownPower.ModifyHandDraw 消耗
        }
        if (player?.Creature == null)
        {
            return;
        }
        SlowdownPower? slowPower = player.Creature.GetPower<SlowdownPower>();
        if (slowPower == null)
        {
            return;
        }
        count = slowPower.Consume(count);
    }
}
