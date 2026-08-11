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

/// <summary>效果卡:点数 4。Bully。效果同原版 Bully。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class BullyFragment : EffectCardModelBase
{
    public BullyFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 4;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Bully>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(4m),
        new ExtraDamageVar(2m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel _, Creature target) => target?.GetPowerAmount<VulnerablePower>() ?? 0),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "ExtraDamage" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        int vulnerable = cardPlay.Target?.GetPowerAmount<VulnerablePower>() ?? 0;
        await DamageCmd.Attack(base.DynamicVars.CalculationBase.BaseValue + UpgradedValue(cardPlay, base.DynamicVars.ExtraDamage.BaseValue, 1m) * vulnerable)
            .FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
    }

}
