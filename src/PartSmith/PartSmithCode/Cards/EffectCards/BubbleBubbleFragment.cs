#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 7。Bubble Bubble。效果同原版 BubbleBubble(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class BubbleBubbleFragment : EffectCardModelBase
{
    public BubbleBubbleFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 7;

    protected override CardModel PortraitSourceCard => ModelDb.Card<BubbleBubble>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<PoisonPower>(9m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "PoisonPower" => 3m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        NCreature nCreature = NCombatRoom.Instance?.GetCreatureNode(cardPlay.Target);
        if (nCreature != null)
        {
            NGaseousImpactVfx child = NGaseousImpactVfx.Create(nCreature.VfxSpawnPosition, new Color("83eb85"));
            NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child);
        }
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        if (cardPlay.Target.HasPower<PoisonPower>())
        {
            await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, UpgradedValue(cardPlay, base.DynamicVars.Poison.BaseValue, 3m), cardPlay.Player.Creature, cardPlay.Card);
        }
    }

}
