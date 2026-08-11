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

/// <summary>效果卡:点数 11。Blaze。效果同原版 Blaze。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class BlazeFragment : EffectCardModelBase
{
    public BlazeFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
    {
    }

    public override int PointCost => 11;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Blaze>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<StrengthPower>(5m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "StrengthPower" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, UpgradedValue(cardPlay, base.DynamicVars.Strength.BaseValue, 2m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
