using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using PartSmith.PartSmithCode.Cards.Base;

namespace PartSmith.PartSmithCode.Cards.Splicing;

/// <summary>
/// 拼接控制器:唯一修改"拼接状态"的地方。负责容量校验、枚举已拼效果、执行拼接。
/// </summary>
public static class SpliceController
{
    /// <summary>宿主卡 <c>_baseStarCost</c> 私有字段(反射写,方案 A 宿主携带星费)。</summary>
    private static readonly FieldInfo BaseStarCostField =
        typeof(CardModel).GetField("_baseStarCost", BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>宿主卡 <c>_starCostSet</c> 惰性初始化标记(反射写 true,防止 BaseStarCost getter 从 canonical 覆盖)。</summary>
    private static readonly FieldInfo StarCostSetField =
        typeof(CardModel).GetField("_starCostSet", BindingFlags.NonPublic | BindingFlags.Instance)!;

    /// <summary>
    /// 宿主拼卡应携带的星费:已拼效果星费之和(仅 StarCost &gt; 0 的效果计入);
    /// 没有星费效果时返回 -1(无星费)。
    /// </summary>
    public static int StarCostOf(CardModel costCard)
    {
        int sum = AttachedEffects(costCard).Sum(e => e.StarCost);
        return sum > 0 ? sum : -1;
    }

    /// <summary>
    /// 方案 A 宿主携带星费:把宿主可变实例的 <c>_baseStarCost</c> 反射写成 <see cref="StarCostOf"/> 的结果,
    /// 并置 <c>_starCostSet = true</c>。基游戏的打出流程随后免费接管一切:
    /// 星费显示 / 星不足灰显(<see cref="UnplayableReason.StarCostTooHigh"/>) / 打出自动 SpendStars /
    /// <c>LastStarsSpent</c> 就位 → <c>ResolveStarXValue()</c> 可用。
    ///
    /// canonical 宿主(无拼卡修饰器)无需处理:<c>BaseStarCost</c> getter 对非 mutable 卡直接返回
    /// <c>CanonicalStarCost</c>(=-1);战斗/牌组里的可变宿主在拼接后必须调用本方法写入。
    /// 拼接/拆拼(以及 parttest 的 AttachUnchecked)都要调,保持星费与已拼效果同步。
    /// </summary>
    public static void RefreshHostStarCost(CardModel costCard)
    {
        if (!costCard.IsMutable)
        {
            return;
        }
        BaseStarCostField.SetValue(costCard, StarCostOf(costCard));
        StarCostSetField.SetValue(costCard, true);
        // 注意:StarCostChanged 是 CardModel 事件,外部类不能 invoke(CS0079);
        // 拼接/拆拼都发生在战斗外或新建战斗实例上,渲染时自会读 CurrentStarCost,无需主动刷新。
    }

    /// <summary>费用卡上已拼效果的点数之和。</summary>
    public static int UsedPoints(CardModel costCard)
        => Attachments(costCard).Sum(m => m.ResolveEffectCard()?.PointCost ?? 0);

    /// <summary>已拼效果(按加入顺序)。</summary>
    public static IEnumerable<EffectCardModelBase> AttachedEffects(CardModel costCard)
        => Attachments(costCard)
            .Select(m => m.ResolveEffectCard())
            .Where(e => e != null)
            .Cast<EffectCardModelBase>();

    /// <summary>费用卡上的拼接修饰器列表(按加入顺序,由 BaseLib 列表序保证)。</summary>
    public static IReadOnlyList<EffectAttachmentModifier> Attachments(CardModel costCard)
        => CardModifier.Modifiers(costCard)
            .OfType<EffectAttachmentModifier>()
            .ToList();

    /// <summary>是否能拼:目标确为费用卡,且 已用点数 + p ≤ 容量 X。</summary>
    public static bool CanSplice(CardModel costCard, EffectCardModelBase effectCard)
        => costCard is CostCardModelBase cost && UsedPoints(costCard) + effectCard.PointCost <= cost.PointCapacity;

    /// <summary>执行拼接:把效果卡拼到费用卡上(校验容量)。成功返回新 modifier,失败返回 null。</summary>
    public static EffectAttachmentModifier? AttachEffect(CardModel costCard, EffectCardModelBase effectCard)
    {
        if (!CanSplice(costCard, effectCard))
        {
            return null;
        }

        int joinIndex = Attachments(costCard).Count;
        var modifier = (EffectAttachmentModifier)CardModifier.Get<EffectAttachmentModifier>().MutableClone();
        modifier.EffectCardId = effectCard.Id.ToString();
        modifier.JoinIndex = joinIndex;
        modifier.Priority = joinIndex;
        CardModifier.AddModifier(costCard, modifier);

        // 效果卡的关键词(如 Exhaust/Innate)转移到宿主拼卡,让拼卡行为与原版一致
        //(拼了"消耗"效果 → 拼卡打出后消耗)。效果卡是共享单例,不能反过来改它的关键词。
        foreach (CardKeyword keyword in effectCard.Keywords)
        {
            costCard.AddKeyword(keyword);
        }

        // 星费(方案 A 宿主携带星费):宿主累加已拼效果的星费,基游戏随后免费接管灰显/扣星。
        RefreshHostStarCost(costCard);
        return modifier;
    }

    /// <summary>是否已拼接过(卡上存在拼接修饰器)。</summary>
    public static bool IsSpliced(CardModel costCard)
        => Attachments(costCard).Count > 0;

    /// <summary>
    /// 拆解:把一张完整拼卡卸成"干净费用卡(留在卡组)+ 全部效果卡(按原顺序返回)"。
    /// 移除全部 <see cref="EffectAttachmentModifier"/>,并对称回收拼接时转移到宿主的关键词
    /// (AttachEffect 里 AddKeyword 幂等,这里 RemoveKeyword 对称幂等)。
    /// 调用前确认 costCard 是卡组里的实例(不在战斗中,可 mutable)。
    /// </summary>
    public static IReadOnlyList<EffectCardModelBase> DetachAllEffects(CardModel costCard)
    {
        var effects = AttachedEffects(costCard).ToList();
        foreach (var m in Attachments(costCard).ToList())
        {
            CardModifier.RemoveModifier(costCard, m);
        }
        foreach (var e in effects)
        {
            foreach (CardKeyword kw in e.Keywords)
            {
                costCard.RemoveKeyword(kw);
            }
        }

        // 星费同步:拆拼后无效果 → 宿主星费复位为 -1(无星费)。
        RefreshHostStarCost(costCard);
        return effects;
    }
}
