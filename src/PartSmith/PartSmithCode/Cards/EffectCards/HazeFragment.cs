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
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 11。Haze。效果同原版 Haze(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class HazeFragment : EffectCardModelBase
{
    public HazeFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 11;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Haze>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<PoisonPower>(4m),
        new PowerVar<WeakPower>(1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "PoisonPower" => 2m, "WeakPower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        Node node = NCombatRoom.Instance?.CombatVfxContainer;
        if (node != null)
        {
            NSmokyVignetteVfx child = NSmokyVignetteVfx.Create(new Color(0.8f, 0.8f, 0.3f, 0.66f), new Color(0f, 4f, 0f, 0.33f));
            node.AddChildSafely(child);
            foreach (Creature hittableEnemy in cardPlay.Card.CombatState.HittableEnemies)
            {
                node.AddChildSafely(NSmokePuffVfx.Create(hittableEnemy, NSmokePuffVfx.SmokePuffColor.Green));
            }
        }
        await Cmd.CustomScaledWait(0.2f, 0.4f);
        await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Card.CombatState?.HittableEnemies, UpgradedValue(cardPlay, base.DynamicVars.Poison.BaseValue, 2m), cardPlay.Player.Creature, cardPlay.Card);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Card.CombatState?.HittableEnemies, UpgradedValue(cardPlay, base.DynamicVars.Weak.BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
