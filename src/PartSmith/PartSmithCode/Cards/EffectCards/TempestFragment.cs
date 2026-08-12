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
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 1。Tempest。效果同原版 Tempest(机器人,X 费)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class TempestFragment : EffectCardModelBase
{
    public TempestFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 1;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Tempest>();

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        int numOrbs = await XCostHelper.ResolveAndSpend(cardPlay);
        if (cardPlay.Card.IsUpgraded)
        {
            numOrbs++;
        }
        for (int i = 0; i < numOrbs; i++)
        {
            await OrbCmd.Channel<LightningOrb>(choiceContext, cardPlay.Player);
        }
    }

}
