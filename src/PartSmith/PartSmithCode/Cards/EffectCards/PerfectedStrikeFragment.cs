#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 10。Perfected Strike。效果同原版 PerfectedStrike。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class PerfectedStrikeFragment : EffectCardModelBase
{
    public PerfectedStrikeFragment() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 10;

    protected override CardModel PortraitSourceCard => ModelDb.Card<PerfectedStrike>();
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Strike };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(6m),
        new ExtraDamageVar(2m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature _) => card.Owner.PlayerCombatState.AllCards.Count((CardModel c) => c.Tags.Contains(CardTag.Strike))),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "ExtraDamage" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        AttackCommand attackCommand = DamageCmd.Attack(PerfectedStrikeDamage(cardPlay)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx(null, null, "heavy_attack.mp3")
            .WithHitVfxNode(t => NBigSlashVfx.Create(t))
            .WithHitVfxNode(t => NBigSlashImpactVfx.Create(t));
        if (PerfectedStrikeDamage(cardPlay) > 12m)
        {
            attackCommand.WithAttackerAnim(Ironclad.GetHeavyAnimIfApplicable(cardPlay.Player.Character), Ironclad.GetHeavyAttackDelayIfApplicable(cardPlay.Player.Character));
        }
        await attackCommand.Execute(choiceContext);
    }


    private decimal PerfectedStrikeDamage(CardPlay cardPlay)
        => base.DynamicVars.CalculationBase.BaseValue
           + UpgradedValue(cardPlay, base.DynamicVars.ExtraDamage.BaseValue, 1m) * cardPlay.Player.PlayerCombatState.AllCards.Count(c => c.Tags.Contains(CardTag.Strike));

}
