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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 9。Quadcast。效果同原版 Quadcast(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class QuadcastFragment : EffectCardModelBase
{
    public QuadcastFragment() : base(0, CardType.Skill, CardRarity.Ancient, TargetType.Self)
    {
    }

    public override int PointCost => 9;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Quadcast>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new RepeatVar(4),
    };
    public override int UpgradeEnergyGain => 1;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Player.PlayerCombatState.OrbQueue.Orbs.Count <= 0)
        {
            return;
        }
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        for (int i = 0; i < base.DynamicVars.Repeat.IntValue; i++)
        {
            await OrbCmd.EvokeNext(choiceContext, cardPlay.Player, i == base.DynamicVars.Repeat.IntValue - 1);
            if (i != base.DynamicVars.Repeat.IntValue - 1)
            {
                await Cmd.CustomScaledWait(0.15f, 0.25f);
            }
        }
    }

}
