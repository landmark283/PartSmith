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
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 4。Spite。效果同原版 Spite。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class SpiteFragment : EffectCardModelBase
{
    public SpiteFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 4;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Spite>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5m, ValueProp.Move),
        new RepeatVar(2),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Repeat" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        int hitCount = LostHpThisTurn(cardPlay.Player.Creature) ? UpgradedIntValue(cardPlay, base.DynamicVars.Repeat.IntValue, 1) : 1;
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(hitCount).FromCard(cardPlay.Card, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }


    private static bool LostHpThisTurn(Creature creature)
        => CombatManager.Instance.History.Entries.OfType<DamageReceivedEntry>().Any(e => e.HappenedThisTurn(creature.CombatState) && e.Receiver == creature && e.Result.UnblockedDamage > 0);

}
