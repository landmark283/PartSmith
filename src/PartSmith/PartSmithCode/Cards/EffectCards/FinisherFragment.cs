#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
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
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Finisher。效果同原版 Finisher(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class FinisherFragment : EffectCardModelBase
{
    public FinisherFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Finisher>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(6m, ValueProp.Move),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedHits").WithMultiplier((CardModel card, Creature _) => CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.HappenedThisTurn(card.CombatState) && e.CardPlay.Card.Type == CardType.Attack && e.CardPlay.Player == card.Owner)),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        int hits = CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e) => e.HappenedThisTurn(cardPlay.Card.CombatState) && e.CardPlay.Card.Type == CardType.Attack && e.CardPlay.Player == cardPlay.Player);
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 2m)).WithHitCount(hits).FromCard(cardPlay.Card, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitVfxNode((Creature t) => NStabVfx.Create(t, facingEnemies: true))
            .WithHitFx(null, null, "blunt_attack.mp3")
            .Execute(choiceContext);
    }

}
