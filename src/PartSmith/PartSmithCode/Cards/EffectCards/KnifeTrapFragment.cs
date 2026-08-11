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
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 12。Knife Trap。效果同原版 KnifeTrap(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class KnifeTrapFragment : EffectCardModelBase
{
    public KnifeTrapFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 12;

    protected override CardModel PortraitSourceCard => ModelDb.Card<KnifeTrap>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("CalculatedShivs").WithMultiplier((CardModel card, Creature _) => PileType.Exhaust.GetPile(card.Owner).Cards.Count((CardModel c) => c.Tags.Contains(CardTag.Shiv))),
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        IEnumerable<CardModel> enumerable = PileType.Exhaust.GetPile(cardPlay.Player).Cards.Where((CardModel c) => c.Tags.Contains(CardTag.Shiv)).ToList();
        bool flag = true;
        foreach (CardModel item in enumerable)
        {
            if (cardPlay.Card.IsUpgraded)
            {
                CardCmd.Upgrade(item, CardPreviewStyle.None);
            }
            await CardCmd.AutoPlay(choiceContext, item, cardPlay.Target, AutoPlayType.Default, skipXCapture: false, !flag);
            flag = false;
        }
    }

}
