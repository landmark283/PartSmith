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
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Darkness。效果同原版 Darkness(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class DarknessFragment : EffectCardModelBase
{
    public DarknessFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Darkness>();

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        await OrbCmd.Channel<DarkOrb>(choiceContext, cardPlay.Player);
        IEnumerable<OrbModel> enumerable = cardPlay.Player.PlayerCombatState.OrbQueue.Orbs.Where((OrbModel orb) => orb is DarkOrb);
        int triggerCount = ((!cardPlay.Card.IsUpgraded) ? 1 : 2);
        foreach (OrbModel darknessOrb in enumerable)
        {
            for (int i = 0; i < triggerCount; i++)
            {
                await OrbCmd.Passive(choiceContext, darknessOrb, null);
            }
        }
    }

}
