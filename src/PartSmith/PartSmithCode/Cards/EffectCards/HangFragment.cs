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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 8。Hang。效果同原版 Hang(亡灵契约师)。</summary>
[Pool(typeof(PartSmithBoneManEffectCardPool))]
public class HangFragment : EffectCardModelBase
{
    public HangFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 8;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Hang>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 3m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        // 吊杀增益:伤害 = 基础伤害 × 目标当前吊杀层数(首打无吊杀 = ×1,与原版一致)。
        // 原版 HangPower.ModifyDamageMultiplicative 只认 cardSource is Hang;拼卡宿主不是 Hang 类,
        // 基游戏的吊杀增益钩子对拼卡不生效(→ 只打基础伤害),这里在 ExecuteEffect 手动乘。
        int hangStacks = cardPlay.Target.GetPowerAmount<HangPower>();
        decimal dmg = UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 3m) * Math.Max(1, hangStacks);
        await DamageCmd.Attack(dmg).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        int powerAmount = cardPlay.Target.GetPowerAmount<HangPower>();
        int num = Math.Max(2, powerAmount);
        if (powerAmount + num > 999999999)
        {
            num = Math.Max(0, 999999999 - powerAmount);
        }
        await PowerCmd.Apply<HangPower>(choiceContext, cardPlay.Target, num, cardPlay.Player.Creature, cardPlay.Card);
    }

    /// <summary>
    /// 吊杀预览:让拼卡描述里的 {Damage} 显示"打向该目标时"的真实数值 = 基础(力量/易伤/虚弱/升级后)× 目标当前吊杀层数。
    /// 原版乘法钩子 HangPower.ModifyDamageMultiplicative 只认 cardSource is Hang,拼卡来源不生效,
    /// ExecuteEffect 已在打出时手动乘层数 → 这里在 base 算完预览后同步乘 PreviewValue,预览与实际一致。
    /// 未选中目标(悬停前)或目标无吊杀(层数 0)→ 不乘 = 显示基础值(首打 ×1,与原版一致)。
    /// base 每次都会把 PreviewValue 重置回 BaseValue,不会跨宿主/跨悬停累积。
    /// </summary>
    public override void RefreshPreviewForHost(CardModel hostCard, Creature target)
    {
        base.RefreshPreviewForHost(hostCard, target);
        if (target != null && target.GetPowerAmount<HangPower>() > 0)
        {
            base.DynamicVars.Damage.PreviewValue *= target.GetPowerAmount<HangPower>();
        }
    }

}
