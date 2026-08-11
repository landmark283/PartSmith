using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using PartSmith.PartSmithCode.Characters;
using PartSmith.PartSmithCode.RestSite;

namespace PartSmith.PartSmithCode.Patches;

/// <summary>
/// 把"钓鱼"选项挂进篝火(仅大战士 / 小猎人)。
/// 注入点 = RestSiteOption.Generate(Player) public static 工厂,
/// RestSiteSynchronizer 每次进篝火对每个玩家调一次;Postfix 追加即可。
/// 与 RewardInjectionPatch 同款 Harmony 注入模式;PatchAll 会自动拾取本类。
/// </summary>
[HarmonyPatch(typeof(RestSiteOption), nameof(RestSiteOption.Generate))]
internal static class RestSiteOptionInjectionPatch
{
    private static void Postfix(List<RestSiteOption> __result, Player player)
    {
        if (player.Character is not (BigWarrior or LittleHunter))
        {
            return; // 只对大战士 / 小猎人生效,不污染原版角色
        }
        __result.Add(new FishingRestSiteOption(player));
    }
}
