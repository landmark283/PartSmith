#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
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

/// <summary>效果卡:点数 5。Primal Force。效果同原版 PrimalForce。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class PrimalForceFragment : EffectCardModelBase
{
    public PrimalForceFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 5;

    protected override CardModel PortraitSourceCard => ModelDb.Card<PrimalForce>();

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        List<CardModel> list = PileType.Hand.GetPile(cardPlay.Player).Cards.Where((CardModel c) => c != null && c.IsTransformable && c.Type == CardType.Attack).ToList();
        foreach (CardModel item in list)
        {
            CardModel cardModel = cardPlay.Card.CombatState.CreateCard<GiantRock>(cardPlay.Player);
            if (cardPlay.Card.IsUpgraded)
            {
                CardCmd.Upgrade(cardModel);
            }
            await CardCmd.Transform(item, cardModel);
        }
    }

}
