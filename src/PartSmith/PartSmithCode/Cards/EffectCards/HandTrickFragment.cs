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

/// <summary>效果卡:点数 7。Hand Trick。效果同原版 HandTrick(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class HandTrickFragment : EffectCardModelBase
{
    public HandTrickFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<HandTrick>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(7m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Block" => 3m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Block.BaseValue, 3m), base.DynamicVars.Block.Props, cardPlay);
        CardModel cardModel = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1), context: choiceContext, player: cardPlay.Player, filter: (CardModel card) => card.Type == CardType.Skill && !card.IsSlyThisTurn, source: cardPlay.Card)).FirstOrDefault();
        if (cardModel != null)
        {
            CardCmd.ApplySingleTurnSly(cardModel);
        }
    }

}
