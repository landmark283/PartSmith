#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
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
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 12。Hyperbeam。效果同原版 Hyperbeam(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class HyperbeamFragment : EffectCardModelBase
{
    public HyperbeamFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 12;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Hyperbeam>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(30m, ValueProp.Move),
        new PowerVar<FocusPower>(3m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 8m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 8m)).FromCard(cardPlay.Card, cardPlay).TargetingAllOpponents(cardPlay.Card.CombatState)
            .WithAttackerAnim("Cast", 0.5f)
            .BeforeDamage(async delegate
            {
                List<Creature> enemies = cardPlay.Card.CombatState.Enemies.Where((Creature e) => e.IsAlive).ToList();
                NHyperbeamVfx nHyperbeamVfx = NHyperbeamVfx.Create(cardPlay.Player.Creature, enemies.Last());
                if (nHyperbeamVfx != null)
                {
                    NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nHyperbeamVfx);
                    await Cmd.Wait(0.5f);
                }
                foreach (Creature item in enemies)
                {
                    NHyperbeamImpactVfx nHyperbeamImpactVfx = NHyperbeamImpactVfx.Create(cardPlay.Player.Creature, item);
                    if (nHyperbeamImpactVfx != null)
                    {
                        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nHyperbeamImpactVfx);
                    }
                }
            })
            .Execute(choiceContext);
        await PowerCmd.Apply<FocusPower>(choiceContext, cardPlay.Player.Creature, -base.DynamicVars["FocusPower"].BaseValue, cardPlay.Player.Creature, cardPlay.Card);
    }

}
