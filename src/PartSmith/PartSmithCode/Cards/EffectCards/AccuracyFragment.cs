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

/// <summary>效果卡:点数 7。Accuracy。效果同原版 Accuracy(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class AccuracyFragment : EffectCardModelBase
{
    public AccuracyFragment() : base(0, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Accuracy>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<AccuracyPower>(4m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "AccuracyPower" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "PowerUp", cardPlay.Player.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<AccuracyPower>(choiceContext, cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars["AccuracyPower"].BaseValue, 2m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
