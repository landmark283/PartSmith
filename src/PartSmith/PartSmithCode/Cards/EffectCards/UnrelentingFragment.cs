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
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 11。Unrelenting。效果同原版 Unrelenting。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class UnrelentingFragment : EffectCardModelBase
{
    public UnrelentingFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 11;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Unrelenting>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(14m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 6m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 6m)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithAttackerAnim(Ironclad.GetHeavyAnimIfApplicable(cardPlay.Player.Character), Ironclad.GetHeavyAttackDelayIfApplicable(cardPlay.Player.Character))
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .WithHitVfxSpawnedAtBase()
            .Execute(choiceContext);
        await PowerCmd.Apply<FreeAttackPower>(choiceContext, cardPlay.Player.Creature, 1m, cardPlay.Player.Creature, cardPlay.Card);
    }

}
