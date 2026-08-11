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

/// <summary>效果卡:点数 8。Dominate。效果同原版 Dominate。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class DominateFragment : EffectCardModelBase
{
    public DominateFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 8;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Dominate>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<VulnerablePower>(1m),
        new DynamicVar("StrengthPerVulnerable", 1m),
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
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, UpgradedValue(cardPlay, base.DynamicVars["VulnerablePower"].BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
        int num = cardPlay.Target.GetPower<VulnerablePower>()?.Amount ?? 0;
        await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Player.Creature, num, cardPlay.Player.Creature, cardPlay.Card);
    }

}
