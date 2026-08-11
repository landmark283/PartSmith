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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 11。Fight Me。效果同原版 FightMe。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class FightMeFragment : EffectCardModelBase
{
    public FightMeFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 11;

    protected override CardModel PortraitSourceCard => ModelDb.Card<FightMe>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5m, ValueProp.Move),
        new RepeatVar(2),
        new PowerVar<StrengthPower>(3m),
        new DynamicVar("EnemyStrength", 1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 1m, "StrengthPower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 1m)).WithHitCount(base.DynamicVars.Repeat.IntValue).FromCard(cardPlay.Card, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
        await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Player.Creature, base.DynamicVars["StrengthPower"].BaseValue, cardPlay.Player.Creature, cardPlay.Card);
        await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, base.DynamicVars["EnemyStrength"].BaseValue, cardPlay.Player.Creature, cardPlay.Card);
    }

}
