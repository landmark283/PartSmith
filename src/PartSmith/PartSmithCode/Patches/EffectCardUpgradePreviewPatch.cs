using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using PartSmith.PartSmithCode.Cards.Base;

namespace PartSmith.PartSmithCode.Patches;

/// <summary>
/// 图鉴升级预览显示效果卡升级数值(v0.1.1 问题1)。
///
/// 根因:效果卡是 canonical 单例、没有宿主拼卡,升级数值抬升只在拼接时由
/// <see cref="EffectCardModelBase.RefreshPreviewForHost"/> 执行(需要宿主上下文);
/// 图鉴里点开"显示升级"时,<c>CardModel.GetDescriptionForUpgradePreview</c> 走原版路径,
/// <c>{Damage:diff()}</c> 等数值仍停留在基础值。
///
/// 方案:prefix hook <c>GetDescriptionForUpgradePreview</c>,当被渲染的是裸效果卡时,
/// 把宿主传它自己调 <see cref="EffectCardModelBase.RefreshPreviewForHost"/>——
/// 图鉴 inspect 已对效果卡 <c>MutableClone</c> + <c>UpgradeInternal</c>(IsUpgraded=true),
/// <c>EffectiveUpgradeLevels</c> 取到级数,各 DynamicVar 被抬到"基础 + 增量"。
///
/// 安全性:只在效果卡被单独展示时生效(拼卡描述走 <c>EffectAttachmentModifier</c>,不经过本方法);
/// 只改 PreviewValue/EnchantedValue(展示用),不改 BaseValue,不跨战斗泄漏。
/// </summary>
[HarmonyPatch(typeof(CardModel))]
internal static class EffectCardUpgradePreviewPatch
{
    [HarmonyPatch("GetDescriptionForUpgradePreview")]
    [HarmonyPrefix]
    private static void LiftUpgradePreviewValues(CardModel __instance)
    {
        if (__instance is EffectCardModelBase effect)
        {
            effect.RefreshPreviewForHost(effect, null);
        }
    }
}
