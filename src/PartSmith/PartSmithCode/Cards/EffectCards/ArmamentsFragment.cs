#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 6。Armaments。效果同原版 Armaments。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class ArmamentsFragment : EffectCardModelBase
{
    public ArmamentsFragment() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCost => 6;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<Armaments>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(5m, ValueProp.Move),
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, base.DynamicVars.Block, cardPlay);
        if (cardPlay.Card.IsUpgraded)
        {
            foreach (CardModel item in PileType.Hand.GetPile(cardPlay.Player).Cards.Where((CardModel c) => c.IsUpgradable))
            {
                CardCmd.Upgrade(item);
            }
            return;
        }
        CardModel cardModel = await CardSelectCmd.FromHandForUpgrade(choiceContext, cardPlay.Player, cardPlay.Card);
        if (cardModel != null)
        {
            CardCmd.Upgrade(cardModel);
        }
    }

}
