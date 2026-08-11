#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Cards.Splicing;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Rampage。效果同原版 Rampage。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class RampageFragment : EffectCardModelBase
{
    public RampageFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Rampage>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(9m, ValueProp.Move),
        new DynamicVar("Increase", 5m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Increase" => 4m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue + RampDamage(cardPlay)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        AddRampDamage(cardPlay, UpgradedValue(cardPlay, base.DynamicVars["Increase"].BaseValue, 4m));
    }


    private static decimal RampDamage(CardPlay cardPlay)
        => CardModifier.Modifiers(cardPlay.Card).OfType<RampExtraModifier>().FirstOrDefault()?.ExtraDamage ?? 0m;

    private static void AddRampDamage(CardPlay cardPlay, decimal amount)
    {
        var modifier = CardModifier.Modifiers(cardPlay.Card).OfType<RampExtraModifier>().FirstOrDefault();
        if (modifier == null)
        {
            modifier = (RampExtraModifier)CardModifier.Get<RampExtraModifier>().MutableClone();
            CardModifier.AddModifier(cardPlay.Card, modifier);
        }
        modifier.ExtraDamage += amount;
    }

}
