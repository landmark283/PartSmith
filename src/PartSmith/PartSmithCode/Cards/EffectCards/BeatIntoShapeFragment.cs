#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
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
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 8。Beat into Shape。效果同原版 BeatIntoShape(储君)。</summary>
[Pool(typeof(PartSmithWangEffectCardPool))]
public class BeatIntoShapeFragment : EffectCardModelBase
{
    public BeatIntoShapeFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 8;

    protected override CardModel PortraitSourceCard => ModelDb.Card<BeatIntoShape>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5m, ValueProp.Move),
        new CalculationBaseVar(5m),
        new CalculationExtraVar(5m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "CalculationBase" => 2m, "CalculationExtra" => 2m, "Damage" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        AttackCommand attackCommand = await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 2m)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        int powered = CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>()
            .Count((DamageReceivedEntry e) => e.Receiver == cardPlay.Target && e.Dealer == cardPlay.Player.Creature && e.Result.Props.IsPoweredAttack() && e.HappenedThisTurn(cardPlay.Card.CombatState));
        decimal amount = UpgradedValue(cardPlay, base.DynamicVars.CalculationBase.BaseValue, 2m) + powered * UpgradedValue(cardPlay, base.DynamicVars.CalculationExtra.BaseValue, 2m) - attackCommand.Results.Count() * UpgradedValue(cardPlay, base.DynamicVars.CalculationExtra.BaseValue, 2m);
        await ForgeCmd.Forge(amount, cardPlay.Player, cardPlay.Card);
    }

}
