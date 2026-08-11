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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 10。Ricochet。效果同原版 Ricochet(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class RicochetFragment : EffectCardModelBase
{
    public RicochetFragment() : base(0, CardType.Attack, CardRarity.Common, TargetType.RandomEnemy)
    {
    }

    public override int PointCost => 10;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Ricochet>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Sly };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(3m, ValueProp.Move),
        new RepeatVar(4),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Repeat" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(UpgradedIntValue(cardPlay, base.DynamicVars.Repeat.IntValue, 1)).FromCard(cardPlay.Card, cardPlay)
            .TargetingRandomOpponents(cardPlay.Card.CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

}
