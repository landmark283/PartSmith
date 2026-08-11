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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 6。Iron Wave。效果同原版 IronWave。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class IronWaveFragment : EffectCardModelBase
{
    public IronWaveFragment() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 6;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<IronWave>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(5m, ValueProp.Move),
        new BlockVar(5m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Block" => 2m, "Damage" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Block.BaseValue, 2m), base.DynamicVars.Block.Props, cardPlay);
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 2m)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_flying_slash")
            .Execute(choiceContext);
    }

}
