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

/// <summary>效果卡:点数 6。Tremble。效果同原版 Tremble。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class TrembleFragment : EffectCardModelBase
{
    public TrembleFragment() : base(0, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 6;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Tremble>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<VulnerablePower>(3m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "VulnerablePower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, UpgradedValue(cardPlay, base.DynamicVars.Vulnerable.BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
