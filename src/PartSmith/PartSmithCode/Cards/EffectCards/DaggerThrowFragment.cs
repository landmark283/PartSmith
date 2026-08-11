#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 6。Dagger Throw。效果同原版 DaggerThrow(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class DaggerThrowFragment : EffectCardModelBase
{
    public DaggerThrowFragment() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 6;

    protected override CardModel PortraitSourceCard => ModelDb.Card<DaggerThrow>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(9m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 3m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 3m)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithAttackerFx(() => NDaggerSprayFlurryVfx.Create(cardPlay.Player.Creature, new Color("#b1ccca"), goingRight: true))
            .WithHitVfxNode((Creature t) => NDaggerSprayImpactVfx.Create(t, new Color("#b1ccca"), goingRight: true))
            .Execute(choiceContext);
        await CardPileCmd.Draw(choiceContext, 1m, cardPlay.Player);
        CardModel cardModel = (await CardSelectCmd.FromHandForDiscard(choiceContext, cardPlay.Player, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1), null, cardPlay.Card)).FirstOrDefault();
        if (cardModel != null)
        {
            await CardCmd.Discard(choiceContext, cardModel);
        }
    }

}
