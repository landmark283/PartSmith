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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Accelerant。效果同原版 Accelerant(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class AccelerantFragment : EffectCardModelBase
{
    public AccelerantFragment() : base(0, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Accelerant>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Accelerant", 1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Accelerant" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "PowerUp", cardPlay.Player.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<AccelerantPower>(choiceContext, cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars["Accelerant"].BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
