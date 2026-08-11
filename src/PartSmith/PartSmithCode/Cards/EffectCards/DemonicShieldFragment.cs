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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 4。Demonic Shield。效果同原版 DemonicShield。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class DemonicShieldFragment : EffectCardModelBase
{
    public DemonicShieldFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
    {
    }

    public override int PointCost => 4;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<DemonicShield>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(0m),
        new HpLossVar(1m),
        new CalculationExtraVar(1m),
        new CalculatedBlockVar(ValueProp.Move).WithMultiplier((CardModel card, Creature _) => card.Owner.Creature.Block),
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        VfxCmd.PlayOnCreatureCenter(cardPlay.Player.Creature, "vfx/vfx_bloody_impact");
        await CreatureCmd.Damage(choiceContext, cardPlay.Player.Creature, base.DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, cardPlay.Card, cardPlay);
        await CreatureCmd.GainBlock(cardPlay.Target, DemonicShieldBlock(cardPlay), ValueProp.Move, cardPlay);
    }


    private decimal DemonicShieldBlock(CardPlay cardPlay)
        => base.DynamicVars.CalculationBase.BaseValue
           + base.DynamicVars.CalculationExtra.BaseValue * cardPlay.Player.Creature.Block;

}
