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

/// <summary>效果卡:点数 4。Bloodletting。效果同原版 Bloodletting。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class BloodlettingFragment : EffectCardModelBase
{
    public BloodlettingFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 4;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Bloodletting>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new HpLossVar(3m),
        new EnergyVar(2),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Energy" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        VfxCmd.PlayOnCreatureCenter(cardPlay.Player.Creature, "vfx/vfx_bloody_impact");
        await CreatureCmd.Damage(choiceContext, cardPlay.Player.Creature, base.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, cardPlay.Card, cardPlay);
        await PlayerCmd.GainEnergy(UpgradedValue(cardPlay, base.DynamicVars.Energy.BaseValue, 1m), cardPlay.Player);
    }

}
