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

/// <summary>效果卡:点数 4。Fade。效果同原版 Fade(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class FadeFragment : EffectCardModelBase
{
    public FadeFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
    {
    }

    public override int PointCost => 4;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Fade>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<DexterityPower>(6m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "DexterityPower" => 3m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await PowerCmd.Apply<FadePower>(choiceContext, cardPlay.Target, UpgradedValue(cardPlay, base.DynamicVars.Dexterity.BaseValue, 3m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
