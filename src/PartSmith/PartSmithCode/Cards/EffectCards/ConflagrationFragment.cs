#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
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

/// <summary>效果卡:点数 8。Conflagration。效果同原版 Conflagration。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class ConflagrationFragment : EffectCardModelBase
{
    public ConflagrationFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 8;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Conflagration>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(2m, ValueProp.Move),
        new RepeatVar(4),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Repeat" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IReadOnlyList<Creature> hittableEnemies = cardPlay.Card.CombatState.HittableEnemies;
        foreach (Creature item in hittableEnemies)
        {
            NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NGroundFireVfx.Create(item));
        }
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(UpgradedIntValue(cardPlay, base.DynamicVars.Repeat.IntValue, 1)).FromCard(cardPlay.Card, cardPlay)
            .TargetingAllOpponents(cardPlay.Card.CombatState)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

}
