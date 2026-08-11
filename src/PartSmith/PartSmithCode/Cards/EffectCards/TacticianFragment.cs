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
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 16。Tactician。效果同原版 Tactician(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class TacticianFragment : EffectCardModelBase
{
    public TacticianFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 16;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Tactician>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Sly };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new EnergyVar(1),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Energy" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(UpgradedIntValue(cardPlay, base.DynamicVars.Energy.IntValue, 1), cardPlay.Player);
    }

}
