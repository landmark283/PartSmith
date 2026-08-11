using System.Collections.Generic;
using System.Linq;
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
        return effects;
    }
}
