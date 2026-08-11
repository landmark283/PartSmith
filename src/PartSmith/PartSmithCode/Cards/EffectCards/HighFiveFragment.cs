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
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 11。High Five。效果同原版 HighFive(亡灵契约师)。</summary>
[Pool(typeof(PartSmithBoneManEffectCardPool))]
public class HighFiveFragment : EffectCardModelBase
{
    public HighFiveFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 11;

    protected override CardModel PortraitSourceCard => ModelDb.Card<HighFive>();
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.OstyAttack };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new OstyDamageVar(11m, ValueProp.Move),
        new PowerVar<VulnerablePower>(2m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "OstyDamage" => 2m, "VulnerablePower" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!Osty.CheckMissingWithAnim(cardPlay.Player))
        {
            await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.OstyDamage.BaseValue, 2m)).FromOsty(cardPlay.Player.Osty, cardPlay.Card, cardPlay).TargetingAllOpponents(cardPlay.Card.CombatState)
                .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
                .Execute(choiceContext);
            await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Card.CombatState?.HittableEnemies, UpgradedValue(cardPlay, base.DynamicVars.Vulnerable.BaseValue, 1m), cardPlay.Player.Creature, cardPlay.Card);
        }
    }

}
