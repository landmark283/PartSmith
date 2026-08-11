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

/// <summary>效果卡:点数 5。Assassinate。效果同原版 Assassinate(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class AssassinateFragment : EffectCardModelBase
{
    public AssassinateFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 5;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Assassinate>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Innate, CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 3m, "VulnerablePower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 3m)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_dramatic_stab")
            .Execute(choiceContext);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, UpgradedValue(cardPlay, base.DynamicVars.Vulnerable.BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
