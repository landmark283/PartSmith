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

/// <summary>效果卡:点数 7。Resonance。效果同原版 Resonance(储君)。</summary>
[Pool(typeof(PartSmithWangEffectCardPool))]
public class ResonanceFragment : EffectCardModelBase
{
    public ResonanceFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 7;

    public override int StarCost => 2;
    protected override CardModel PortraitSourceCard => ModelDb.Card<Resonance>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<StrengthPower>(1m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "StrengthPower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "Cast", cardPlay.Player.Character.CastAnimDelay);
        int intValue = UpgradedIntValue(cardPlay, base.DynamicVars["StrengthPower"].IntValue, 1);
        await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Player.Creature, intValue, cardPlay.Player.Creature, cardPlay.Card);
        foreach (Creature hittableEnemy in cardPlay.Card.CombatState.HittableEnemies)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, hittableEnemy, -1m, cardPlay.Player.Creature, cardPlay.Card);
        }
    }

}
