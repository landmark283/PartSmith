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
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 5。Decisions, Decisions。效果同原版 DecisionsDecisions(储君)。</summary>
[Pool(typeof(PartSmithWangEffectCardPool))]
public class DecisionsDecisionsFragment : EffectCardModelBase
{
    public DecisionsDecisionsFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 5;

    public override int StarCost => 6;
    protected override CardModel PortraitSourceCard => ModelDb.Card<DecisionsDecisions>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CardsVar(3),
        new RepeatVar(3),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Cards" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        await CardPileCmd.Draw(choiceContext, UpgradedIntValue(cardPlay, base.DynamicVars.Cards.IntValue, 2), cardPlay.Player);
        CardSelectorPrefs prefs = new CardSelectorPrefs(base.SelectionScreenPrompt, 1)
        {
            PretendCardsCanBePlayed = true
        };
        CardModel card = (await CardSelectCmd.FromHand(choiceContext, cardPlay.Player, prefs, (CardModel c) => c.Type == CardType.Skill && !c.Keywords.Contains(CardKeyword.Unplayable), cardPlay.Card)).FirstOrDefault();
        if (card != null)
        {
            for (int i = 0; i < base.DynamicVars.Repeat.IntValue; i++)
            {
                await CardCmd.AutoPlay(choiceContext, card, null);
            }
        }
    }

}
