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
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 12。Consuming Shadow。效果同原版 ConsumingShadow(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class ConsumingShadowFragment : EffectCardModelBase
{
    public ConsumingShadowFragment() : base(0, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 12;

    protected override CardModel PortraitSourceCard => ModelDb.Card<ConsumingShadow>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new RepeatVar(2),
        new PowerVar<ConsumingShadowPower>(1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Repeat" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "PowerUp", cardPlay.Player.Character.PowerUpAnimDelay);
        for (int i = 0; i < UpgradedIntValue(cardPlay, base.DynamicVars.Repeat.IntValue, 1); i++)
        {
            await OrbCmd.Channel<DarkOrb>(choiceContext, cardPlay.Player);
        }
        await PowerCmd.Apply<ConsumingShadowPower>(choiceContext, cardPlay.Player.Creature, base.DynamicVars["ConsumingShadowPower"].BaseValue, cardPlay.Player.Creature, cardPlay.Card);
    }

}
