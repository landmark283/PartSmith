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
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 5。Offering。效果同原版 Offering。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class OfferingFragment : EffectCardModelBase
{
    public OfferingFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 5;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Offering>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new HpLossVar(6m),
        new EnergyVar(2),
        new CardsVar(3),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Cards" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(choiceContext, cardPlay.Player.Creature, base.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, cardPlay.Card, cardPlay);
        await PlayerCmd.GainEnergy(base.DynamicVars.Energy.IntValue, cardPlay.Player);
        await CardPileCmd.Draw(choiceContext, UpgradedValue(cardPlay, base.DynamicVars.Cards.BaseValue, 2m), cardPlay.Player);
    }

}
