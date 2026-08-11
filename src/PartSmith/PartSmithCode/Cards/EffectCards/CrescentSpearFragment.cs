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
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 6。Crescent Spear。效果同原版 CrescentSpear(储君)。</summary>
[Pool(typeof(PartSmithWangEffectCardPool))]
public class CrescentSpearFragment : EffectCardModelBase
{
    public CrescentSpearFragment() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 6;

    public override int StarCost => 1;
    protected override CardModel PortraitSourceCard => ModelDb.Card<CrescentSpear>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(8m),
        new ExtraDamageVar(2m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "ExtraDamage" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        int starCards = cardPlay.Player.PlayerCombatState.AllCards.Count((CardModel c) => c.CanonicalStarCost >= 0 || c.HasStarCostX);
        await DamageCmd.Attack(base.DynamicVars.CalculationBase.BaseValue + UpgradedValue(cardPlay, base.DynamicVars.ExtraDamage.BaseValue, 1m) * starCards).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_starry_impact")
            .Execute(choiceContext);
    }

}
