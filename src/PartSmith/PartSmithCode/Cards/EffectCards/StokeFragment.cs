#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 8。Stoke。效果同原版 Stoke。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class StokeFragment : EffectCardModelBase
{
    public StokeFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 8;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Stoke>();

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        List<CardModel> list = PileType.Hand.GetPile(cardPlay.Player).Cards.ToList();
        int exhaustCount = list.Count;
        foreach (CardModel item in list)
        {
            await CardCmd.Exhaust(choiceContext, item);
        }
        List<CardModel> cards = CardFactory.GetForCombat(cardPlay.Player, cardPlay.Player.Character.CardPool.GetUnlockedCards(cardPlay.Player.UnlockState, cardPlay.Player.RunState.CardMultiplayerConstraint), exhaustCount, cardPlay.Player.RunState.Rng.CombatCardGeneration).ToList();
        if (cardPlay.Card.IsUpgraded)
        {
            CardCmd.Upgrade(cards, CardPreviewStyle.None);
        }
        await CardPileCmd.AddGeneratedCardsToCombat(cards, PileType.Hand, cardPlay.Player);
    }

}
