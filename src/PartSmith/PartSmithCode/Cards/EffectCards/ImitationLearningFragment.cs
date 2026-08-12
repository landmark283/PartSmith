#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 8。Imitation Learning。效果同原版 ImitationLearning(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class ImitationLearningFragment : EffectCardModelBase
{
    public ImitationLearningFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
    {
    }

    public override int PointCost => 8;

    protected override CardModel PortraitSourceCard => ModelDb.Card<ImitationLearning>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<ImitationLearningPower>(2m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "ImitationLearningPower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "PowerUp", cardPlay.Player.Character.PowerUpAnimDelay);
        ImitationLearningPower imitationLearningPower = cardPlay.Player.Creature.Powers.OfType<ImitationLearningPower>().FirstOrDefault((ImitationLearningPower s) => s.PlayerTarget == cardPlay.Target.Player);
        decimal baseValue = UpgradedValue(cardPlay, base.DynamicVars["ImitationLearningPower"].BaseValue, 1m);
        if (imitationLearningPower != null)
        {
            await PowerCmd.ModifyAmount(choiceContext, imitationLearningPower, baseValue, cardPlay.Player.Creature, cardPlay.Card);
            return;
        }
        imitationLearningPower = await PowerCmd.Apply<ImitationLearningPower>(choiceContext, cardPlay.Player.Creature, baseValue, cardPlay.Player.Creature, cardPlay.Card);
        if (imitationLearningPower != null)
        {
            imitationLearningPower.PlayerTarget = cardPlay.Target.Player;
        }
    }

}
