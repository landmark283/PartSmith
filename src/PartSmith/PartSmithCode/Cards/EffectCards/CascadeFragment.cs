#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 1。Cascade。效果同原版 Cascade(大战士,X 费)。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class CascadeFragment : EffectCardModelBase
{
    public CascadeFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 1;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Cascade>();

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = await XCostHelper.ResolveAndSpend(cardPlay);
        if (cardPlay.Card.IsUpgraded)
        {
            x++;
        }
        await CardPileCmd.AutoPlayFromDrawPile(choiceContext, cardPlay.Player, x, CardPilePosition.Top, forceExhaust: false);
    }

}
