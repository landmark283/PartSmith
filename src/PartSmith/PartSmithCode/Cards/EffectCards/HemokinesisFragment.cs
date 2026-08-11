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

/// <summary>效果卡:点数 7。Hemokinesis。效果同原版 Hemokinesis。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class HemokinesisFragment : EffectCardModelBase
{
    public HemokinesisFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Hemokinesis>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new HpLossVar(2m),
        new DamageVar(15m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 5m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await CreatureCmd.Damage(choiceContext, cardPlay.Player.Creature, base.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, cardPlay.Card, cardPlay);
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 5m)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_bloody_impact")
            .Execute(choiceContext);
    }

}
