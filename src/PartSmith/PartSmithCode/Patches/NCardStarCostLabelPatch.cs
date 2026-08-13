using Godot;
using HarmonyLib;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Nodes.Cards;
using PartSmith.PartSmithCode.Cards.Base;

namespace PartSmith.PartSmithCode.Patches;

/// <summary>
/// 效果卡卡面显示所需星辉(v0.1.1 问题2,储君"能量 + 辉星"双轨费)。
///
/// 根因:原版 NCard 的星费节点(%StarLabel/%StarIcon)只按
/// <c>CardModel.GetStarCostWithModifiers()</c> 显示,效果卡是 canonical 单例、该值恒 -1
/// → 星费图标被隐藏,玩家在效果卡奖励/图鉴预览里看不到这张效果卡拼上后要花几颗星。
/// mod 的 <see cref="EffectCardModelBase.StarCost"/> 只在拼接时由
/// <c>SpliceController.RefreshHostStarCost</c> 写进宿主费用卡,裸效果卡从不把星费喂给渲染器。
///
/// 方案:postfix hook <c>NCard.UpdateVisuals</c>,当 Model 是裸效果卡时点亮原版星节点、
/// 填入 <c>effect.StarCost</c>。因为 NCard 是所有卡面渲染共用节点,这一个 patch 覆盖全部界面
/// (3 选 1 奖励屏、图鉴、检视、篝火拼卡、拼卡选牌)。宿主拼卡的星费走原版路径,不经过这里;
/// 非储君效果卡 StarCost 恒 0,自动隐藏,其余角色无影响。
/// </summary>
[HarmonyPatch(typeof(NCard))]
internal static class NCardStarCostLabelPatch
{
    [HarmonyPatch("UpdateVisuals")]
    [HarmonyPostfix]
    private static void ApplyEffectStarCost(NCard __instance)
    {
        if (__instance.Model is not EffectCardModelBase effect)
        {
            return;
        }

        var starLabel = __instance.GetNodeOrNull<MegaLabel>("%StarLabel");
        var starIcon = __instance.GetNodeOrNull<TextureRect>("%StarIcon");
        if (starLabel == null || starIcon == null)
        {
            return;
        }

        if (effect.StarCost > 0)
        {
            starLabel.SetTextAutoSize(effect.StarCost.ToString());
            starIcon.Visible = true;
        }
        else
        {
            starLabel.SetTextAutoSize(string.Empty);
            starIcon.Visible = false;
        }
    }
}
