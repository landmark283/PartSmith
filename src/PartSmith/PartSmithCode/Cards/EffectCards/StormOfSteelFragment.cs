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
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 8。Storm of Steel。效果同原版 StormOfSteel(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class StormOfSteelFragment : EffectCardModelBase
{
    public StormOfSteelFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 8;

    protected override CardModel PortraitSourceCard => ModelDb.Card<StormOfSteel>();

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IEnumerable<CardModel> enumerable = PileType.Hand.GetPile(cardPlay.Player).Cards.ToList();
        int handSize = enumerable.Count();
        await CardCmd.Discard(choiceContext, enumerable);
        await Cmd.CustomScaledWait(0f, 0.25f);
        IEnumerable<CardModel> enumerable2 = await Shiv.CreateInHand(cardPlay.Player, handSize, cardPlay.Card.CombatState);
        if (!cardPlay.Card.IsUpgraded)
        {
            return;
        }
        foreach (CardModel item in enumerable2)
        {
            CardCmd.Upgrade(item);
        }
    }

}
