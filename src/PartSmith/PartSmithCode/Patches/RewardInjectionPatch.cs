using System.Collections.Generic;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using PartSmith.PartSmithCode.Characters;
using PartSmith.PartSmithCode.Rewards;

namespace PartSmith.PartSmithCode.Patches;

/// <summary>
/// 战斗奖励注入:把"大战士 / 小猎人 / 王 / 骨头人 / 机器人"的战斗卡牌奖励替换成
/// 费用卡奖励(CardReward,3 选 1)+ 效果卡奖励(SpliceReward,3 选 1 → 拼到费用卡上)。
///
/// 普通小怪房(Monster)= 1 费用卡 + 1 效果卡;精英房(Elite)= 1 费用卡 + 2 效果卡;
/// Boss 保留原版。
/// 大战士走 PartSmithCostCardPool/PartSmithEffectCardPool(ironclad 图标);
/// 小猎人走 PartSmithHunterCostCardPool/PartSmithHunterEffectCardPool(silent 绿图标);
/// 王走 PartSmithWangCostCardPool/PartSmithWangEffectCardPool(regent 橙图标);
/// 骨头人走 PartSmithBoneManCostCardPool/PartSmithBoneManEffectCardPool(necrobinder 粉图标);
/// 机器人走 PartSmithRobotCostCardPool/PartSmithRobotEffectCardPool(defect 蓝图标)。
/// 注入点 = RewardsSet.GenerateRewardsFor(Player, AbstractRoom) private 实例方法,
/// 此刻奖励尚未 Populate,可安全替换(ref __result)。
/// </summary>
[HarmonyPatch(typeof(RewardsSet), "GenerateRewardsFor")]
internal static class RewardInjectionPatch
{
    private static void Postfix(List<Reward> __result, Player player, AbstractRoom room)
    {
        // 只对大战士 / 小猎人 / 王 / 骨头人 / 机器人生效,不污染原版角色。
        if (player.Character is not (BigWarrior or LittleHunter or Wang or BoneMan or Robot))
        {
            return;
        }

        // 普通小怪 + 精英都替换;Boss 保留原版。
        if (room.RoomType is not (RoomType.Monster or RoomType.Elite))
        {
            return;
        }

        bool hunter = player.Character is LittleHunter;
        bool wang = player.Character is Wang;
        bool boneMan = player.Character is BoneMan;
        bool robot = player.Character is Robot;

        // 把原版卡牌奖励槽替换成"费用卡奖励 + 效果卡奖励"。排序交给 RewardsSetIndex(金币=1/药水=2/费用=5/效果=6)。
        // 普通小怪 = 1 费用 + 1 效果;精英 = 1 费用 + 2 效果(第二个效果奖励与第一个同 RewardsSetIndex,稳定排序相邻)。
        // 每个槽各自 new 一个奖励对象(同一 SpliceReward 实例放两个槽会让奖励状态机共享状态)。
        for (int i = 0; i < __result.Count; i++)
        {
            if (__result[i] is CardReward)
            {
                __result[i] = CostReward(player, hunter, wang, boneMan, robot);
                __result.Insert(i + 1, EffectReward(player, room.RoomType, hunter, wang, boneMan, robot));
                if (room.RoomType == RoomType.Elite)
                {
                    __result.Insert(i + 2, EffectReward(player, room.RoomType, hunter, wang, boneMan, robot));
                }
                return;
            }
        }
    }

    /// <summary>费用卡奖励按角色分派池:大战士=ironclad 池 / 小猎人=silent 绿池 / 王=regent 橙池 / 骨头人=necrobinder 粉池 / 机器人=defect 蓝池。</summary>
    private static CardReward CostReward(Player player, bool hunter, bool wang, bool boneMan, bool robot)
        => robot ? PartSmithRewardFactory.CreateRobotCostCardReward(player)
        : boneMan ? PartSmithRewardFactory.CreateBoneManCostCardReward(player)
        : wang ? PartSmithRewardFactory.CreateWangCostCardReward(player)
        : hunter ? PartSmithRewardFactory.CreateHunterCostCardReward(player)
        : PartSmithRewardFactory.CreateCostCardReward(player);

    /// <summary>效果卡奖励按角色分派池,同上。</summary>
    private static SpliceReward EffectReward(Player player, RoomType roomType, bool hunter, bool wang, bool boneMan, bool robot)
        => robot ? PartSmithRewardFactory.CreateRobotEffectCardReward(player, roomType)
        : boneMan ? PartSmithRewardFactory.CreateBoneManEffectCardReward(player, roomType)
        : wang ? PartSmithRewardFactory.CreateWangEffectCardReward(player, roomType)
        : hunter ? PartSmithRewardFactory.CreateHunterEffectCardReward(player, roomType)
        : PartSmithRewardFactory.CreateEffectCardReward(player, roomType);
}
