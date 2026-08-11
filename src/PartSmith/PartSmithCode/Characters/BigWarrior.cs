using System.Collections.Generic;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;

namespace PartSmith.PartSmithCode.Characters;

/// <summary>
/// 大战士:战士的 fork。除了"卡池/奖励 = 拼接体系"外,全部照抄战士:
/// PlaceholderID="ironclad" → 视觉/动画/音效/能量计数器/角色选择界面全复用战士资源(BaseLib 官方捷径)。
///
/// 与战士的差异只有一处:CardPool = PartSmithCostCardPool(费用卡专属池),
/// 战斗奖励由 M2a 的注入换成"费用卡池 + 效果卡池"双槽。
/// </summary>
public class BigWarrior : PlaceholderCharacterModel
{
    public override string PlaceholderID => "ironclad";

    public override CharacterGender Gender => CharacterGender.Masculine;

    public override Color NameColor => StsColors.red;

    public override int StartingHp => 80;

    // 奖励注入(M2a)已接管怪物/精英房的战斗奖励(费用池+效果池双槽),本字段只影响
    // 未被注入覆盖的场景(商店、事件等)。仍沿用战士卡池以免这些场景拿到费用壳/效果卡;
    public override CardPoolModel CardPool => ModelDb.CardPool<IroncladCardPool>();

    public override PotionPoolModel PotionPool => ModelDb.PotionPool<IroncladPotionPool>();

    public override RelicPoolModel RelicPool => ModelDb.RelicPool<IroncladRelicPool>();

    public override IEnumerable<CardModel> StartingDeck => new CardModel[]
    {
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<Bash>()
    };

    public override IReadOnlyList<RelicModel> StartingRelics => new[] { ModelDb.Relic<BurningBlood>() };
}
