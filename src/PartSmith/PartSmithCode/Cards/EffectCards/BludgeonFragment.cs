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
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 16。Bludgeon。效果同原版 Bludgeon。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class BludgeonFragment : EffectCardModelBase
{
    public BludgeonFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 16;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Bludgeon>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(32m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 10m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 10m)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithAttackerAnim(Ironclad.GetHeavyAnimIfApplicable(cardPlay.Player.Character), Ironclad.GetHeavyAttackDelayIfApplicable(cardPlay.Player.Character))
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .WithHitVfxSpawnedAtBase()
            .Execute(choiceContext);
    }

}
