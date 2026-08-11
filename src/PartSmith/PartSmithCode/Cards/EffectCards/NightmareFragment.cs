#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.Nodes.Vfx.Cards;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 17。Nightmare。效果同原版 Nightmare(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class NightmareFragment : EffectCardModelBase
{
    public NightmareFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 17;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Nightmare>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    public override int UpgradeEnergyGain => 1;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        IEnumerable<CardModel> cards = await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(base.SelectionScreenPrompt, 1), context: choiceContext, player: cardPlay.Player, filter: null, source: cardPlay.Card);
        if (TestMode.IsOff && LocalContext.IsMe(cardPlay.Player))
        {
            NSmokyVignetteVfx child = NSmokyVignetteVfx.Create(new Color(0.8f, 0.3f, 0.8f, 0.66f), new Color(0f, 0f, 4f, 0.33f));
            NGame.Instance.CurrentRunNode.GlobalUi.AddChildSafely(child);
            NGame.Instance.CurrentRunNode.GlobalUi.AddChildSafely(NNightmareHandsVfx.Create());
            await Cmd.CustomScaledWait(0.1f, 0.25f);
        }
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        CardModel selectedCard = cards.FirstOrDefault();
        if (selectedCard != null)
        {
            (await PowerCmd.Apply<NightmarePower>(choiceContext, cardPlay.Player.Creature, 3m, cardPlay.Player.Creature, cardPlay.Card)).SetSelectedCard(selectedCard);
        }
    }

}
