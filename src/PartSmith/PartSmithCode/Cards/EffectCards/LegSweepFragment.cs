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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 11。Leg Sweep。效果同原版 LegSweep(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class LegSweepFragment : EffectCardModelBase
{
    public LegSweepFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 11;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<LegSweep>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(11m, ValueProp.Move),
        new PowerVar<WeakPower>(2m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Block" => 3m, "WeakPower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Block.BaseValue, 3m), base.DynamicVars.Block.Props, cardPlay);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, UpgradedValue(cardPlay, base.DynamicVars.Weak.BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
