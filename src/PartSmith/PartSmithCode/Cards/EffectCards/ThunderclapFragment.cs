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

/// <summary>效果卡:点数 6。Thunderclap。效果同原版 Thunderclap。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class ThunderclapFragment : EffectCardModelBase
{
    public ThunderclapFragment() : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 6;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Thunderclap>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(4m, ValueProp.Move),
        new PowerVar<VulnerablePower>(1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 3m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 3m)).FromCard(cardPlay.Card, cardPlay).TargetingAllOpponents(cardPlay.Card.CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Card.CombatState?.HittableEnemies, base.DynamicVars.Vulnerable.BaseValue, cardPlay.Player.Creature, cardPlay.Card);
    }

}
