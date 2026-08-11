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
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 17。Squeeze。效果同原版 Squeeze(亡灵契约师)。</summary>
[Pool(typeof(PartSmithBoneManEffectCardPool))]
public class SqueezeFragment : EffectCardModelBase
{
    public SqueezeFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 17;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Squeeze>();
    protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.OstyAttack };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(25m),
        new ExtraDamageVar(5m).FromOsty(),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "CalculationBase" => 5m, "ExtraDamage" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        if (!Osty.CheckMissingWithAnim(cardPlay.Player))
        {
            int ostyAttackCards = cardPlay.Player.PlayerCombatState.AllCards.Count((CardModel c) => c.Tags.Contains(CardTag.OstyAttack) && c != cardPlay.Card);
            decimal dmg = UpgradedValue(cardPlay, base.DynamicVars.CalculationBase.BaseValue, 5m) + UpgradedValue(cardPlay, base.DynamicVars.ExtraDamage.BaseValue, 1m) * ostyAttackCards;
            await DamageCmd.Attack(dmg).FromOsty(cardPlay.Player.Osty, cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt", null, "blunt_attack.mp3")
                .Execute(choiceContext);
        }
    }

}
