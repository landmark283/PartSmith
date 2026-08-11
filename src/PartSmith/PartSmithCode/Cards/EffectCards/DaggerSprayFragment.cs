#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 6。Dagger Spray。效果同原版 DaggerSpray(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class DaggerSprayFragment : EffectCardModelBase
{
    public DaggerSprayFragment() : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 6;

    protected override CardModel PortraitSourceCard => ModelDb.Card<DaggerSpray>();
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
        SfxCmd.Play("event:/sfx/characters/silent/silent_dagger_spray");
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 2m)).WithHitCount(2).FromCard(cardPlay.Card, cardPlay)
            .TargetingAllOpponents(cardPlay.Card.CombatState)
            .WithAttackerFx(() => NDaggerSprayFlurryVfx.Create(cardPlay.Player.Creature, new Color("#b1ccca"), goingRight: true))
            .BeforeDamage(delegate
            {
                IReadOnlyList<Creature> hittableEnemies = cardPlay.Card.CombatState.HittableEnemies;
                foreach (Creature item in hittableEnemies)
                {
                    NDaggerSprayImpactVfx child = NDaggerSprayImpactVfx.Create(item, new Color("#b1ccca"), goingRight: true);
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(child);
                }
                return Task.CompletedTask;
            })
            .Execute(choiceContext);
    }

}
