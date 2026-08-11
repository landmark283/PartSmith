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
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 4。Precise Cut。效果同原版 PreciseCut(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class PreciseCutFragment : EffectCardModelBase
{
    public PreciseCutFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 4;

    protected override CardModel PortraitSourceCard => ModelDb.Card<PreciseCut>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(13m),
        new ExtraDamageVar(2m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier(delegate(CardModel card, Creature _)
        {
            int num = PileType.Hand.GetPile(card.Owner).Cards.Count;
            CardPile pile = card.Pile;
            if (pile != null && pile.Type == PileType.Hand)
            {
                num--;
            }
            return -num;
        }),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "CalculationBase" => 3m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await DamageCmd.Attack(base.DynamicVars.CalculatedDamage).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash", null, "dagger_throw.mp3")
            .Execute(choiceContext);
    }

}
