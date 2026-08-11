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
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 9。Protector。效果同原版 Protector(亡灵契约师)。</summary>
[Pool(typeof(PartSmithBoneManEffectCardPool))]
public class ProtectorFragment : EffectCardModelBase
{
    public ProtectorFragment() : base(0, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 9;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Protector>();
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.OstyAttack };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(10m),
        new ExtraDamageVar(1m).FromOsty(),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "CalculationBase" => 5m,
        _ => 0m,
    };
    public override int UpgradeEnergyGain => 1;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        if (!Osty.CheckMissingWithAnim(cardPlay.Player))
        {
            decimal ostyMax = cardPlay.Player.Osty?.IsAlive == true ? cardPlay.Player.Osty.MaxHp : 0;
            await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.CalculationBase.BaseValue, 5m) + base.DynamicVars.ExtraDamage.BaseValue * ostyMax).FromOsty(cardPlay.Player.Osty, cardPlay.Card, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
                .Execute(choiceContext);
        }
    }

}
