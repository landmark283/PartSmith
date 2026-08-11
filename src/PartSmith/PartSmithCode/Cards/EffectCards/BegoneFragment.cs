#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
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

/// <summary>效果卡:点数 6。BEGONE!。效果同原版 Begone(储君)。</summary>
[Pool(typeof(PartSmithWangEffectCardPool))]
public class BegoneFragment : EffectCardModelBase
{
    public BegoneFragment() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCost => 6;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Begone>();

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel cardModel = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1), context: choiceContext, player: cardPlay.Player, filter: null, source: cardPlay.Card)).FirstOrDefault();
        if (cardModel != null)
        {
            CardModel cardModel2 = cardPlay.Card.CombatState.CreateCard<MinionStrike>(cardPlay.Player);
            if (cardPlay.Card.IsUpgraded)
            {
                CardCmd.Upgrade(cardModel2);
            }
            await CardCmd.Transform(cardModel, cardModel2);
        }
    }

}
