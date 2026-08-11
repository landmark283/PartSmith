#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 11。Flame Barrier。效果同原版 FlameBarrier。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class FlameBarrierFragment : EffectCardModelBase
{
    public FlameBarrierFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 11;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<FlameBarrier>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(12m, ValueProp.Move),
        new DynamicVar("DamageBack", 4m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Block" => 4m, "DamageBack" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        NFireBurningVfx child = NFireBurningVfx.Create(cardPlay.Player.Creature, 0.75f, goingRight: false);
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(child);
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Block.BaseValue, 4m), base.DynamicVars.Block.Props, cardPlay);
        await PowerCmd.Apply<FlameBarrierPower>(choiceContext, cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars["DamageBack"].BaseValue, 2m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
