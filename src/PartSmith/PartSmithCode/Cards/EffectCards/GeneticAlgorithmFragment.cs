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
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Cards.Splicing;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 8。Genetic Algorithm。效果同原版 GeneticAlgorithm(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class GeneticAlgorithmFragment : EffectCardModelBase
{
    public GeneticAlgorithmFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 8;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<GeneticAlgorithm>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(1m, ValueProp.Move),
        new IntVar("Increase", 3m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Increase" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal block = base.DynamicVars.Block.BaseValue + GenAlgoGrowth(cardPlay.Card);
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, block, base.DynamicVars.Block.Props, cardPlay);
        int increase = UpgradedIntValue(cardPlay, base.DynamicVars["Increase"].IntValue, 1);
        AddGenAlgoGrowth(cardPlay.Card, increase);
        if (cardPlay.Card.DeckVersion is { } deckVersion)
        {
            AddGenAlgoGrowth(deckVersion, increase);
        }
    }


    private static int GenAlgoGrowth(CardModel card)
        => CardModifier.Modifiers(card).OfType<GenAlgoExtraModifier>().FirstOrDefault()?.IncreasedBlock ?? 0;

    private static void AddGenAlgoGrowth(CardModel card, int amount)
    {
        var modifier = CardModifier.Modifiers(card).OfType<GenAlgoExtraModifier>().FirstOrDefault();
        if (modifier == null)
        {
            modifier = (GenAlgoExtraModifier)CardModifier.Get<GenAlgoExtraModifier>().MutableClone();
            CardModifier.AddModifier(card, modifier);
        }
        modifier.IncreasedBlock += amount;
    }

}
