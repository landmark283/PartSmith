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

/// <summary>效果卡:点数 6。Havoc。效果同原版 Havoc。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class HavocFragment : EffectCardModelBase
{
    public HavocFragment() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCost => 6;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Havoc>();
    public override int UpgradeEnergyGain => 1;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.AutoPlayFromDrawPile(choiceContext, cardPlay.Player, 1, CardPilePosition.Top, forceExhaust: true);
    }

}
