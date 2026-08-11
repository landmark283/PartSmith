#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 6。Deadly Poison。效果同原版 DeadlyPoison(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class DeadlyPoisonFragment : EffectCardModelBase
{
    public DeadlyPoisonFragment() : base(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 6;

    protected override CardModel PortraitSourceCard => ModelDb.Card<DeadlyPoison>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<PoisonPower>(5m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "PoisonPower" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        NPoisonImpactVfx child = NPoisonImpactVfx.Create(cardPlay.Target);
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(child);
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, UpgradedValue(cardPlay, base.DynamicVars.Poison.BaseValue, 2m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
