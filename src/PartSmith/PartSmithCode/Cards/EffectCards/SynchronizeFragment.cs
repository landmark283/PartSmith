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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Synchronize。效果同原版 Synchronize(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class SynchronizeFragment : EffectCardModelBase
{
    public SynchronizeFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Synchronize>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationExtraVar(2m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "CalculationExtra" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int focus = cardPlay.Player.PlayerCombatState.OrbQueue.Orbs.GroupBy((OrbModel o) => o.Id).Count()
            * ((int)base.DynamicVars.CalculationExtra.BaseValue + EffectiveUpgradeLevels(cardPlay.Card));
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        await PowerCmd.Apply<SynchronizePower>(choiceContext, cardPlay.Player.Creature, focus, cardPlay.Player.Creature, cardPlay.Card);
    }

}
