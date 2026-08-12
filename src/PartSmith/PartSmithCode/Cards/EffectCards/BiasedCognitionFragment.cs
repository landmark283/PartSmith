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

/// <summary>效果卡:点数 9。Biased Cognition。效果同原版 BiasedCognition(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class BiasedCognitionFragment : EffectCardModelBase
{
    public BiasedCognitionFragment() : base(0, CardType.Power, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override int PointCost => 9;

    protected override CardModel PortraitSourceCard => ModelDb.Card<BiasedCognition>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<FocusPower>(5m),
        new PowerVar<BiasedCognitionPower>(1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "FocusPower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "PowerUp", cardPlay.Player.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<FocusPower>(choiceContext, cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars["FocusPower"].BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
        await PowerCmd.Apply<BiasedCognitionPower>(choiceContext, cardPlay.Player.Creature, base.DynamicVars["BiasedCognitionPower"].BaseValue, cardPlay.Player.Creature, cardPlay.Card);
    }

}
