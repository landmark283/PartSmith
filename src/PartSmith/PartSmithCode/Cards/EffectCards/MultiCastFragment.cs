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
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 1。Multi-Cast。效果同原版 MultiCast(机器人,X 费)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class MultiCastFragment : EffectCardModelBase
{
    public MultiCastFragment() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 1;

    protected override CardModel PortraitSourceCard => ModelDb.Card<MultiCast>();

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        int evokeCount = await XCostHelper.ResolveAndSpend(cardPlay);
        if (cardPlay.Card.IsUpgraded)
        {
            evokeCount++;
        }
        for (int i = 0; i < evokeCount; i++)
        {
            await OrbCmd.EvokeNext(choiceContext, cardPlay.Player, i == evokeCount - 1);
            await Cmd.Wait(0.25f);
        }
    }

}
