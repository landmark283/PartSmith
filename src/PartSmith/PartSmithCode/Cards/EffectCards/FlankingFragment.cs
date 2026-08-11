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
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 12。Flanking。效果同原版 Flanking(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class FlankingFragment : EffectCardModelBase
{
    public FlankingFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 12;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Flanking>();
    public override int UpgradeEnergyGain => 1;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        await PowerCmd.Apply<FlankingPower>(choiceContext, cardPlay.Target, 2m, cardPlay.Player.Creature, cardPlay.Card);
    }

}
