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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 4。Radiate。效果同原版 Radiate(储君)。</summary>
[Pool(typeof(PartSmithWangEffectCardPool))]
public class RadiateFragment : EffectCardModelBase
{
    public RadiateFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 4;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Radiate>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
        new StarsVar(1),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int gained = CombatManager.Instance.History.Entries.OfType<StarsModifiedEntry>()
            .Where((StarsModifiedEntry e) => e.HappenedThisTurn(cardPlay.Card.CombatState) && e.Amount > 0 && e.Actor == cardPlay.Player.Creature)
            .Sum((StarsModifiedEntry e) => (int)e.Amount);
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 1m)).WithHitCount(gained).FromCard(cardPlay.Card, cardPlay)
            .TargetingAllOpponents(cardPlay.Card.CombatState)
            .WithHitFx("vfx/vfx_starry_impact", null, "slash_attack.mp3")
            .SpawningHitVfxOnEachCreature()
            .Execute(choiceContext);
    }

}
