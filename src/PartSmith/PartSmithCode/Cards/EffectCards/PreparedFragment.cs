#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
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
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 3。Prepared。效果同原版 Prepared(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class PreparedFragment : EffectCardModelBase
{
    public PreparedFragment() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCost => 3;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Prepared>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(1),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Cards" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int cardCount = UpgradedIntValue(cardPlay, base.DynamicVars.Cards.IntValue, 1);
        await CardPileCmd.Draw(choiceContext, cardCount, cardPlay.Player);
        await CardCmd.Discard(choiceContext, await CardSelectCmd.FromHandForDiscard(choiceContext, cardPlay.Player, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, cardCount), null, cardPlay.Card));
    }

}
