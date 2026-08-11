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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Ashen Strike。效果同原版 AshenStrike。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class AshenStrikeFragment : EffectCardModelBase
{
    public AshenStrikeFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<AshenStrike>();
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(6m),
        new ExtraDamageVar(3m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature _) => PileType.Exhaust.GetPile(card.Owner).Cards.Count),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "ExtraDamage" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await DamageCmd.Attack(AshenStrikeDamage(cardPlay))
            .FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash", null, "blunt_attack.mp3")
            .Execute(choiceContext);
    }


    private decimal AshenStrikeDamage(CardPlay cardPlay)
        => base.DynamicVars.CalculationBase.BaseValue
           + UpgradedValue(cardPlay, base.DynamicVars.ExtraDamage.BaseValue, 1m) * PileType.Exhaust.GetPile(cardPlay.Player).Cards.Count;

}
