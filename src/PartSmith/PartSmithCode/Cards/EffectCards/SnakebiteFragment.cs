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

/// <summary>效果卡:点数 10。Snakebite。效果同原版 Snakebite(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class SnakebiteFragment : EffectCardModelBase
{
    public SnakebiteFragment() : base(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 10;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Snakebite>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<PoisonPower>(7m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "PoisonPower" => 3m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        VfxCmd.PlayOnCreatureCenter(cardPlay.Target, "vfx/vfx_bite");
        await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, UpgradedValue(cardPlay, base.DynamicVars.Poison.BaseValue, 3m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
