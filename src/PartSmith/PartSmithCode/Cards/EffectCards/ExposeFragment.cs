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
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 4。Expose。效果同原版 Expose(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class ExposeFragment : EffectCardModelBase
{
    public ExposeFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 4;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Expose>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DynamicVar("Power", 2m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Power" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        VfxCmd.PlayOnCreatureCenter(cardPlay.Player.Creature, "vfx/vfx_flying_slash");
        int amount = UpgradedIntValue(cardPlay, base.DynamicVars["Power"].IntValue, 1);
        await CreatureCmd.LoseBlock(choiceContext, cardPlay.Target, cardPlay.Target.Block, cardPlay.Player.Creature);
        if (cardPlay.Target.HasPower<ArtifactPower>())
        {
            await PowerCmd.Remove<ArtifactPower>(cardPlay.Target);
        }
        await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, amount, cardPlay.Player.Creature, cardPlay.Card);
    }

}
