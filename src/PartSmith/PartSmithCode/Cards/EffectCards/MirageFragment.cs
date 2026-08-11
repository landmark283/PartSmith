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

/// <summary>效果卡:点数 7。Mirage。效果同原版 Mirage(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class MirageFragment : EffectCardModelBase
{
    public MirageFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 7;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<Mirage>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((CardModel card, Creature _) => card.CombatState?.Enemies.Where((Creature c) => c.IsAlive).Sum((Creature c) => c.GetPowerAmount<PoisonPower>()) ?? 0),
    };
    public override int UpgradeEnergyGain => 1;

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal block = cardPlay.Card.CombatState?.Enemies.Where((Creature c) => c.IsAlive).Sum((Creature c) => c.GetPowerAmount<PoisonPower>()) ?? 0;
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, block, base.DynamicVars.CalculatedBlock.Props, cardPlay);
    }

}
