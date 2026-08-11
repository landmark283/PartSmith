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
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 11。Uppercut。效果同原版 Uppercut。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class UppercutFragment : EffectCardModelBase
{
    public UppercutFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 11;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Uppercut>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(13m, ValueProp.Move),
        new DynamicVar("Power", 1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Power" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithAttackerAnim(Ironclad.GetHeavyAnimIfApplicable(cardPlay.Player.Character), Ironclad.GetHeavyAttackDelayIfApplicable(cardPlay.Player.Character))
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .WithHitVfxSpawnedAtBase()
            .Execute(choiceContext);
        int amount = UpgradedIntValue(cardPlay, base.DynamicVars["Power"].IntValue, 1);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, amount, cardPlay.Player.Creature, cardPlay.Card);
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, amount, cardPlay.Player.Creature, cardPlay.Card);
    }

}
