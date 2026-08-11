#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 8。Feed。效果同原版 Feed。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class FeedFragment : EffectCardModelBase
{
    public FeedFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 8;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Feed>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move),
        new MaxHpVar(3m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 2m, "MaxHp" => 1m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        bool shouldTriggerFatal = cardPlay.Target.Powers.All((PowerModel p) => p.ShouldOwnerDeathTriggerFatal());
        AttackCommand attackCommand = await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 2m)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_bite", null, "blunt_attack.mp3")
            .Execute(choiceContext);
        if (shouldTriggerFatal && attackCommand.Results.SelectMany((List<DamageResult> r) => r).Any((DamageResult r) => r.WasTargetKilled))
        {
            await CreatureCmd.GainMaxHp(cardPlay.Player.Creature, UpgradedIntValue(cardPlay, base.DynamicVars.MaxHp.IntValue, 1));
        }
    }

}
