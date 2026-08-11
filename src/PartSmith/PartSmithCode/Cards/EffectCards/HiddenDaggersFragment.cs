#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
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
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 4。Hidden Daggers。效果同原版 HiddenDaggers(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class HiddenDaggersFragment : EffectCardModelBase
{
    public HiddenDaggersFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 4;

    protected override CardModel PortraitSourceCard => ModelDb.Card<HiddenDaggers>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(2),
        new DynamicVar("Shivs", 2m),
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardCmd.Discard(choiceContext, await CardSelectCmd.FromHandForDiscard(prefs: new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, base.DynamicVars.Cards.IntValue), context: choiceContext, player: cardPlay.Player, filter: null, source: cardPlay.Card));
        IEnumerable<CardModel> enumerable = await Shiv.CreateInHand(cardPlay.Player, base.DynamicVars["Shivs"].IntValue, cardPlay.Card.CombatState);
        if (!cardPlay.Card.IsUpgraded)
        {
            return;
        }
        foreach (CardModel item in enumerable)
        {
            CardCmd.Upgrade(item);
        }
    }

}
