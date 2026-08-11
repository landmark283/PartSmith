#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 10。Glitterstream。效果同原版 Glitterstream(储君)。</summary>
[Pool(typeof(PartSmithWangEffectCardPool))]
public class GlitterstreamFragment : EffectCardModelBase
{
    public GlitterstreamFragment() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCost => 10;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<Glitterstream>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(11m, ValueProp.Move),
        new BlockVar("BlockNextTurn", 5m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Block" => 2m, "BlockNextTurn" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        BlockVar blockVar = (BlockVar)base.DynamicVars["BlockNextTurn"];
        decimal blockNextTurnAmount = Hook.ModifyBlock(cardPlay.Card.CombatState, cardPlay.Player.Creature, blockVar.BaseValue, blockVar.Props, cardPlay.Card, cardPlay, out IEnumerable<AbstractModel> _);
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Block.BaseValue, 2m), base.DynamicVars.Block.Props, cardPlay);
        await PowerCmd.Apply<BlockNextTurnPower>(choiceContext, cardPlay.Player.Creature, blockNextTurnAmount, cardPlay.Player.Creature, cardPlay.Card);
    }

}
