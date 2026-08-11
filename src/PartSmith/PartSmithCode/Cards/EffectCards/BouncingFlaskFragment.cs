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
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 11。Bouncing Flask。效果同原版 BouncingFlask(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class BouncingFlaskFragment : EffectCardModelBase
{
    public BouncingFlaskFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.RandomEnemy)
    {
    }

    public override int PointCost => 11;

    protected override CardModel PortraitSourceCard => ModelDb.Card<BouncingFlask>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<PoisonPower>(3m),
        new RepeatVar(3),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Repeat" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        Vector2 lastPos = Vector2.Zero;
        for (int i = 0; i < UpgradedIntValue(cardPlay, base.DynamicVars.Repeat.IntValue, 1); i++)
        {
            Creature enemy = cardPlay.Player.RunState.Rng.CombatTargets.NextItem(cardPlay.Card.CombatState.HittableEnemies);
            if (enemy == null)
            {
                continue;
            }
            if (TestMode.IsOff)
            {
                if (i == 0)
                {
                    lastPos = NCombatRoom.Instance.GetCreatureNode(cardPlay.Player.Creature).VfxSpawnPosition;
                }
                NCreature targetNode = NCombatRoom.Instance.GetCreatureNode(enemy);
                if (targetNode != null)
                {
                    NItemThrowVfx child = NItemThrowVfx.Create(lastPos, targetNode.GetBottomOfHitbox(), ModelDb.Potion<PoisonPotion>().Image);
                    NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child);
                    lastPos = targetNode.VfxSpawnPosition;
                    await Cmd.Wait(0.5f);
                    NSplashVfx child2 = NSplashVfx.Create(targetNode.VfxSpawnPosition, new Color("83eb85"));
                    NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child2);
                    NLiquidOverlayVfx child3 = NLiquidOverlayVfx.Create(enemy, new Color("83eb85"));
                    NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child3);
                    NGaseousImpactVfx child4 = NGaseousImpactVfx.Create(targetNode.VfxSpawnPosition, new Color("83eb85"));
                    NCombatRoom.Instance.CombatVfxContainer.AddChildSafely(child4);
                }
            }
            await PowerCmd.Apply<PoisonPower>(choiceContext, enemy, base.DynamicVars.Poison.BaseValue, cardPlay.Player.Creature, cardPlay.Card);
        }
    }

}
