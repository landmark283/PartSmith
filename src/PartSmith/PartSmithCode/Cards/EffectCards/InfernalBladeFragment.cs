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
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Infernal Blade。效果同原版 InfernalBlade。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class InfernalBladeFragment : EffectCardModelBase
{
    public InfernalBladeFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<InfernalBlade>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    public override int UpgradeEnergyGain => 1;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel cardModel = CardFactory.GetDistinctForCombat(cardPlay.Player, from c in cardPlay.Player.Character.CardPool.GetUnlockedCards(cardPlay.Player.UnlockState, cardPlay.Player.RunState.CardMultiplayerConstraint)
            where c.Type == CardType.Attack
            select c, 1, cardPlay.Player.RunState.Rng.CombatCardGeneration).FirstOrDefault();
        if (cardModel != null)
        {
            cardModel.SetToFreeThisTurn();
            await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Hand, cardPlay.Player);
        }
    }

}
