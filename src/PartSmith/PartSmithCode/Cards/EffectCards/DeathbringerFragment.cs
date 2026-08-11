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
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 11。Deathbringer。效果同原版 Deathbringer(亡灵契约师)。</summary>
[Pool(typeof(PartSmithBoneManEffectCardPool))]
public class DeathbringerFragment : EffectCardModelBase
{
    public DeathbringerFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 11;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Deathbringer>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<DoomPower>(21m),
        new PowerVar<WeakPower>(1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "DoomPower" => 5m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        await PowerCmd.Apply<DoomPower>(choiceContext, cardPlay.Card.CombatState?.HittableEnemies, UpgradedValue(cardPlay, base.DynamicVars.Doom.BaseValue, 5m), cardPlay.Player.Creature, cardPlay.Card);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Card.CombatState?.HittableEnemies, base.DynamicVars.Weak.BaseValue, cardPlay.Player.Creature, cardPlay.Card);
    }

}
