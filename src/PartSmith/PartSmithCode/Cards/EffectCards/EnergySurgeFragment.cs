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

/// <summary>效果卡:点数 7。Energy Surge。效果同原版 EnergySurge(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class EnergySurgeFragment : EffectCardModelBase
{
    public EnergySurgeFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllAllies)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<EnergySurge>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(2),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Energy" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "PowerUp", cardPlay.Player.Character.PowerUpAnimDelay);
        IEnumerable<Creature> enumerable = from c in cardPlay.Card.CombatState.GetTeammatesOf(cardPlay.Player.Creature)
            where c != null && c.IsAlive && c.IsPlayer
            select c;
        foreach (Creature item in enumerable)
        {
            await PlayerCmd.GainEnergy(UpgradedIntValue(cardPlay, base.DynamicVars.Energy.IntValue, 1), item.Player);
        }
    }

}
