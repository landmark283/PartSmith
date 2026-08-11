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
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 12。Seven Stars。效果同原版 SevenStars(储君)。</summary>
[Pool(typeof(PartSmithWangEffectCardPool))]
public class SevenStarsFragment : EffectCardModelBase
{
    public SevenStarsFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 12;

    public override int StarCost => 7;
    protected override CardModel PortraitSourceCard => ModelDb.Card<SevenStars>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(7m, ValueProp.Move),
        new RepeatVar(7),
    };
    public override int UpgradeEnergyGain => 1;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(base.DynamicVars.Repeat.IntValue).FromCard(cardPlay.Card, cardPlay)
            .TargetingAllOpponents(cardPlay.Card.CombatState)
            .WithHitFx("vfx/vfx_starry_impact", null, "slash_attack.mp3")
            .SpawningHitVfxOnEachCreature()
            .Execute(choiceContext);
    }

}
