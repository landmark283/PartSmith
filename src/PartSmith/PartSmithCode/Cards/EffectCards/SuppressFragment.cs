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
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Settings;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 6。Suppress。效果同原版 Suppress(猎人)。</summary>
[Pool(typeof(PartSmithHunterEffectCardPool))]
public class SuppressFragment : EffectCardModelBase
{
    public SuppressFragment() : base(0, CardType.Attack, CardRarity.Ancient, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 6;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Suppress>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Innate };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(11m, ValueProp.Move),
        new PowerVar<WeakPower>(3m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 6m, "WeakPower" => 2m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(NThinSliceVfx.Create(cardPlay.Target));
        float num = cardPlay.Player.Character.AttackAnimDelay;
        if (SaveManager.Instance.PrefsSave.FastMode == FastModeType.Normal)
        {
            num += 0.2f;
        }
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 6m)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithAttackerAnim("Attack", num)
            .Execute(choiceContext);
        await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, UpgradedValue(cardPlay, base.DynamicVars.Weak.BaseValue, 2m), cardPlay.Player.Creature, cardPlay.Card);
    }

}
