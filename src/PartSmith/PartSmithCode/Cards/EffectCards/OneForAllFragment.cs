#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 8。One for All。效果同原版 OneForAll(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class OneForAllFragment : EffectCardModelBase
{
    public OneForAllFragment() : base(0, CardType.Power, CardRarity.Rare, TargetType.AllAllies)
    {
    }

    public override int PointCost => 8;

    protected override CardModel PortraitSourceCard => ModelDb.Card<OneForAll>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<OneForAllPower>(3m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "OneForAllPower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (Player player in cardPlay.Card.CombatState?.Players ?? Array.Empty<Player>())
        {
            await CreatureCmd.TriggerAnim(player.Creature, "PowerUp", player.Character.PowerUpAnimDelay);
            await PowerCmd.Apply<OneForAllPower>(choiceContext, player.Creature, UpgradedValue(cardPlay, base.DynamicVars["OneForAllPower"].BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
        }
    }

}
