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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 8。One-Two Punch。效果同原版 OneTwoPunch。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class OneTwoPunchFragment : EffectCardModelBase
{
    public OneTwoPunchFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 8;

    protected override CardModel PortraitSourceCard => ModelDb.Card<OneTwoPunch>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Attacks", 1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Attacks" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<OneTwoPunchPower>(choiceContext, cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars["Attacks"].BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
