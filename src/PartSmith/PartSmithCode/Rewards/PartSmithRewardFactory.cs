using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Rewards;

/// <summary>
/// 生成大战士的两种战斗奖励:费用卡奖励 + 效果卡奖励(各自 3 选 1,可跳过)。
/// 都用池构造(Encounter 来源),保证 CardReward.ToSerializable 可序列化:
/// 必须有 CardPools、不能有 filter、flags 只允许 IsCardReward。
///
/// 稀有度权重(学习官方 CardFactory.RollForRarity + CardRarityOdds):
/// - 费用卡池:EliteEncounter(普通:罕见:稀有 = 50:40:10,即 5:4:1)
/// - 效果卡池:按房间——普通小怪房 RegularEncounter(60:37:3),精英房 EliteEncounter(50:40:10)
/// </summary>
public static class PartSmithRewardFactory
{
    /// <summary>费用卡奖励:从 PartSmithCostCardPool 出 3 张 3 选 1,选中的费用卡直接入组(原版 CardReward)。</summary>
    public static CardReward CreateCostCardReward(Player player)
    {
        var options = new CardCreationOptions(
            new[] { ModelDb.CardPool<PartSmithCostCardPool>() },
            CardCreationSource.Encounter,
            CardRarityOddsType.EliteEncounter);
        return new CardReward(options, 3, player);
    }

    /// <summary>效果卡奖励:从 PartSmithEffectCardPool 出 3 张 3 选 1,选完拼到卡组里的费用卡上。稀有度权重按房间:Monster=RegularEncounter,Elite=EliteEncounter。</summary>
    public static SpliceReward CreateEffectCardReward(Player player, RoomType roomType)
    {
        var options = new CardCreationOptions(
            new[] { ModelDb.CardPool<PartSmithEffectCardPool>() },
            CardCreationSource.Encounter,
            roomType == RoomType.Elite ? CardRarityOddsType.EliteEncounter : CardRarityOddsType.RegularEncounter);
        return new SpliceReward(options, 3, player);
    }

    /// <summary>小猎人费用卡奖励:从 PartSmithHunterCostCardPool 出 3 张 3 选 1(猎人绿费用图标)。</summary>
    public static CardReward CreateHunterCostCardReward(Player player)
    {
        var options = new CardCreationOptions(
            new[] { ModelDb.CardPool<PartSmithHunterCostCardPool>() },
            CardCreationSource.Encounter,
            CardRarityOddsType.EliteEncounter);
        return new CardReward(options, 3, player);
    }

    /// <summary>小猎人效果卡奖励:从 PartSmithHunterEffectCardPool 出 3 张 3 选 1,拼到小猎人卡组里的费用卡上。</summary>
    public static SpliceReward CreateHunterEffectCardReward(Player player, RoomType roomType)
    {
        var options = new CardCreationOptions(
            new[] { ModelDb.CardPool<PartSmithHunterEffectCardPool>() },
            CardCreationSource.Encounter,
            roomType == RoomType.Elite ? CardRarityOddsType.EliteEncounter : CardRarityOddsType.RegularEncounter);
        return new SpliceReward(options, 3, player);
    }

    /// <summary>王(储君)费用卡奖励:从 PartSmithWangCostCardPool 出 3 张 3 选 1(regent 橙费用图标)。</summary>
    public static CardReward CreateWangCostCardReward(Player player)
    {
        var options = new CardCreationOptions(
            new[] { ModelDb.CardPool<PartSmithWangCostCardPool>() },
            CardCreationSource.Encounter,
            CardRarityOddsType.EliteEncounter);
        return new CardReward(options, 3, player);
    }

    /// <summary>王(储君)效果卡奖励:从 PartSmithWangEffectCardPool 出 3 张 3 选 1,拼到王卡组里的费用卡上。
    /// 稀有度权重按房间:Monster=RegularEncounter,Elite=EliteEncounter。</summary>
    public static SpliceReward CreateWangEffectCardReward(Player player, RoomType roomType)
    {
        var options = new CardCreationOptions(
            new[] { ModelDb.CardPool<PartSmithWangEffectCardPool>() },
            CardCreationSource.Encounter,
            roomType == RoomType.Elite ? CardRarityOddsType.EliteEncounter : CardRarityOddsType.RegularEncounter);
        return new SpliceReward(options, 3, player);
    }

    /// <summary>骨头人费用卡奖励:从 PartSmithBoneManCostCardPool 出 3 张 3 选 1(necrobinder 粉费用图标)。</summary>
    public static CardReward CreateBoneManCostCardReward(Player player)
    {
        var options = new CardCreationOptions(
            new[] { ModelDb.CardPool<PartSmithBoneManCostCardPool>() },
            CardCreationSource.Encounter,
            CardRarityOddsType.EliteEncounter);
        return new CardReward(options, 3, player);
    }

    /// <summary>骨头人效果卡奖励:从 PartSmithBoneManEffectCardPool 出 3 张 3 选 1,拼到骨头人卡组里的费用卡上。
    /// 稀有度权重按房间:Monster=RegularEncounter,Elite=EliteEncounter。</summary>
    public static SpliceReward CreateBoneManEffectCardReward(Player player, RoomType roomType)
    {
        var options = new CardCreationOptions(
            new[] { ModelDb.CardPool<PartSmithBoneManEffectCardPool>() },
            CardCreationSource.Encounter,
            roomType == RoomType.Elite ? CardRarityOddsType.EliteEncounter : CardRarityOddsType.RegularEncounter);
        return new SpliceReward(options, 3, player);
    }
}
