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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 8。Dying Star。效果同原版 DyingStar(储君)。</summary>
[Pool(typeof(PartSmithWangEffectCardPool))]
public class DyingStarFragment : EffectCardModelBase
{
    public DyingStarFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 8;

    public override int StarCost => 3;
    protected override CardModel PortraitSourceCard => ModelDb.Card<DyingStar>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(9m, ValueProp.Move),
        new DynamicVar("StrengthLoss", 9m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 2m, "StrengthLoss" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Attack", cardPlay.Player.Character.AttackAnimDelay);
        IReadOnlyList<Creature> enemies = cardPlay.Card.CombatState.HittableEnemies;
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 2m)).FromCard(cardPlay.Card, cardPlay).TargetingAllOpponents(cardPlay.Card.CombatState)
            .WithHitFx("vfx/vfx_starry_impact")
            .SpawningHitVfxOnEachCreature()
            .Execute(choiceContext);
        foreach (Creature enemy in enemies)
        {
            await PowerCmd.Apply<DyingStarPower>(choiceContext, enemy, UpgradedValue(cardPlay, base.DynamicVars["StrengthLoss"].BaseValue, 2m), cardPlay.Player.Creature, cardPlay.Card);
            VfxCmd.PlayOnCreature(enemy, "vfx/vfx_attack_slash");
        }
    }

}
