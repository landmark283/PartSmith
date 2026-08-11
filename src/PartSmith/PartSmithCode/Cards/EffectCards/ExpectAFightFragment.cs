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

/// <summary>效果卡:点数 11。Expect A Fight。效果同原版 ExpectAFight。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class ExpectAFightFragment : EffectCardModelBase
{
    public ExpectAFightFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 11;

    protected override CardModel PortraitSourceCard => ModelDb.Card<ExpectAFight>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(0),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedEnergy").WithMultiplier((CardModel card, Creature _) => PileType.Hand.GetPile(card.Owner).Cards.Count((CardModel c) => c.Type == CardType.Attack)),
    };
    public override int UpgradeEnergyGain => 1;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        await PlayerCmd.GainEnergy((int)ExpectAFightEnergy(cardPlay), cardPlay.Player);
    }


    private decimal ExpectAFightEnergy(CardPlay cardPlay)
        => base.DynamicVars.CalculationBase.BaseValue
           + base.DynamicVars.CalculationExtra.BaseValue * PileType.Hand.GetPile(cardPlay.Player).Cards.Count(c => c.Type == CardType.Attack);

}
