using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数消耗 2,打出拼卡时获得 2 点格挡(普通)。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class GuardFragment : EffectCardModelBase
{
    public GuardFragment() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCost => 2;

    protected override CardModel? PortraitSourceCard => ModelDb.Card<DefendIronclad>();

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, 2m, ValueProp.Move, cardPlay);
    }
}
