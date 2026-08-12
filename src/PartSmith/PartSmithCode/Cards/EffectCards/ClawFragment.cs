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
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Cards.Splicing;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 3。Claw。效果同原版 Claw(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class ClawFragment : EffectCardModelBase
{
    public ClawFragment() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 3;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Claw>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
        new DynamicVar("Increase", 2m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 1m, "Increase" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 1m) + ClawDamage(cardPlay.Card)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitVfxNode((Creature t) => NScratchVfx.Create(t, goingRight: true))
            .Execute(choiceContext);
        decimal increase = UpgradedValue(cardPlay, base.DynamicVars["Increase"].BaseValue, 1m);
        string effectId = this.Id.ToString();
        foreach (CardModel c in cardPlay.Player.PlayerCombatState.AllCards.Where((CardModel c) => IsClawCard(c, effectId)).ToList())
        {
            AddClawDamage(c, increase);
        }
    }


    private static bool IsClawCard(CardModel card, string effectId)
        => CardModifier.Modifiers(card).OfType<EffectAttachmentModifier>().Any((EffectAttachmentModifier m) => m.EffectCardId == effectId);

    private static decimal ClawDamage(CardModel card)
        => CardModifier.Modifiers(card).OfType<ClawExtraModifier>().FirstOrDefault()?.ExtraDamage ?? 0m;

    private static void AddClawDamage(CardModel card, decimal amount)
    {
        var modifier = CardModifier.Modifiers(card).OfType<ClawExtraModifier>().FirstOrDefault();
        if (modifier == null)
        {
            modifier = (ClawExtraModifier)CardModifier.Get<ClawExtraModifier>().MutableClone();
            CardModifier.AddModifier(card, modifier);
        }
        modifier.ExtraDamage += amount;
    }

}
