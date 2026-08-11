#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Cards.Splicing;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 8。Thrash。效果同原版 Thrash。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class ThrashFragment : EffectCardModelBase
{
    public ThrashFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 8;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Thrash>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(4m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 2m) + RampDamage(cardPlay)).WithHitCount(2).FromCard(cardPlay.Card, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_thrash")
            .Execute(choiceContext);
        CardPile pile = PileType.Hand.GetPile(cardPlay.Player);
        CardModel cardModel = cardPlay.Player.RunState.Rng.CombatCardSelection.NextItem(pile.Cards.Where(c => c.Type == CardType.Attack));
        if (cardModel != null)
        {
            decimal damage = 0m;
            if (cardModel.DynamicVars.ContainsKey("CalculatedDamage"))
            {
                damage = cardModel.DynamicVars.CalculatedDamage.Calculate(null);
            }
            else if (cardModel.DynamicVars.ContainsKey("Damage"))
            {
                damage = cardModel.DynamicVars.Damage.BaseValue;
            }
            else if (cardModel.DynamicVars.ContainsKey("OstyDamage"))
            {
                damage = cardModel.DynamicVars.OstyDamage.BaseValue;
            }
            else
            {
                Log.Warn(cardPlay.Card.Id.Entry + " exhausted attack card " + cardModel.Id.Entry + " that did not have an appropriate damage var!");
            }
            damage = Hook.ModifyDamage(cardPlay.Player.RunState, cardPlay.Player.Creature.CombatState, null, cardPlay.Player.Creature, damage, ValueProp.Move, cardModel, null, ModifyDamageHookType.All, CardPreviewMode.None, out IEnumerable<AbstractModel> _);
            AddRampDamage(cardPlay, damage);
            await CardCmd.Exhaust(choiceContext, cardModel);
        }
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
