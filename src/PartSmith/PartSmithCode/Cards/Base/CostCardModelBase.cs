using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using PartSmith.PartSmithCode.Cards.Splicing;

namespace PartSmith.PartSmithCode.Cards.Base;

/// <summary>
/// 费用卡片基类:一张普通卡(正常能量费用/类型/稀有度)+ 一个"点数容量 X"。
/// 空壳本身没有任何效果;效果卡片通过 <see cref="EffectAttachmentModifier"/> 拼接到它上面,
/// 拼好的卡打出时:先走费用卡自身 OnPlay(默认无),再按加入顺序执行所有已拼效果。
///
/// 拼卡后的显示按需求动态生成:
/// - 名字   = 所有效果卡名字,逗号连接(空壳 = 本名)
/// - 卡面   = 第一张效果卡的图(空壳 = 本卡图)
/// - 目标类型 = 由效果派生(拼了需要目标的攻击效果 → 打牌时需要选目标)
/// </summary>
public abstract class CostCardModelBase(int cost, CardType type, CardRarity rarity, TargetType target) :
    PartSmithCard(cost, type, rarity, target, showInCardLibrary: true)
{
    /// <summary>点数容量 X:可拼接的效果点数总和上限。</summary>
    public virtual int PointCapacity => 0;

    /// <summary>拼卡名 = 效果卡名字逗号连接;空壳 = 费用卡本名。兼容原版升级 + 后缀。</summary>
    public override string Title
    {
        get
        {
            var effects = SpliceController.AttachedEffects(this).ToList();
            string name = effects.Count > 0
                ? string.Join(", ", effects.Select(e => e.TitleLocString.GetFormattedText()))
                : TitleLocString.GetFormattedText();

            if (!IsUpgraded)
            {
                return name;
            }
            return MaxUpgradeLevel > 1 ? $"{name}+{CurrentUpgradeLevel}" : name + "+";
        }
    }

    /// <summary>
    /// X 星费(方案 A 宿主携带星费,Stardust 用):拼了任意带 <c>HasStarCostX</c> 的效果卡 → 宿主也是 X 星费。
    /// 基游戏随后按 X 处理:费用 = 当前星数(<see cref="CardModel.GetStarCostWithModifiers"/> 的 HasStarCostX 分支),
    /// 打出耗光所有星,<c>LastStarsSpent = 打出时星数</c> → <c>ResolveStarXValue()</c> 可用。
    /// </summary>
    public override bool HasStarCostX => SpliceController.AttachedEffects(this).Any(e => e.HasStarCostX);

    /// <summary>卡面 = 第一张效果卡的图;空壳 = 本卡占位图。</summary>
    public override string PortraitPath
    {
        get
        {
            var first = SpliceController.AttachedEffects(this).FirstOrDefault();
            return first != null ? first.PortraitPath : base.PortraitPath;
        }
    }

    /// <summary>
    /// 拼卡卡面:按已拼效果卡实时合成(规则见 card_art_splicing_plan.md)。
    /// 0 张 = 空壳,返回 null 走 CustomPortraitPath/PortraitPath 原占位路径;
    /// 1 张 = 第一张效果卡原画整张;2 张 = 中间分开(左/右各半);
    /// 3 张 = 三等分;≥4 张 = 维持三拼(取前 3)。
    ///
    /// 机制:CardModel.Portrait 是非 virtual getter,BaseLib 的 CustomCardPortrait patch
    /// 拦截它,当 <see cref="CustomPortrait"/> 非 null 时直接返回——这就是运行时纹理 hook。
    /// 带签名缓存:效果卡组合不变就返回缓存的 Texture2D,不重复合成。
    /// 合成失败(个别卡无原画 PNG)时回退第一张效果卡的原版卡面,避免空白卡。
    /// </summary>
    public override Texture2D? CustomPortrait
    {
        get
        {
            var fx = SpliceController.AttachedEffects(this).ToList();
            if (fx.Count == 0)
            {
                return null;
            }
            string key = string.Join(">", fx.Select(e => e.Id.Entry));
            if (_artCacheKey == key)
            {
                return _artCache;
            }
            _artCacheKey = key;
            _artCache = CardArtSplicer.Build(fx) ?? fx[0].SourcePortraitTexture;
            return _artCache;
        }
    }

    private string? _artCacheKey;
    private Texture2D? _artCache;

    /// <summary>
    /// 拼卡类型由效果派生(用户定:能力 &gt; 攻击 &gt; 技能)。
    /// 有 Power 效果 → Power;否则有 Attack 效果 → Attack;否则回落壳的基础类型(技能)。
    /// 让按类型分类的内容(如 SelfHelpBook 附魔按攻击/技能/能力筛卡)能正确识别拼卡。
    /// </summary>
    public override CardType Type
    {
        get
        {
            var effects = SpliceController.AttachedEffects(this).ToList();
            if (effects.Any(e => e.Type == CardType.Power))
            {
                return CardType.Power;
            }
            if (effects.Any(e => e.Type == CardType.Attack))
            {
                return CardType.Attack;
            }
            return base.Type;
        }
    }

    /// <summary>拼了格挡效果就算格挡牌(供按"格挡牌"触发的效果/AI 识别)。</summary>
    public override bool GainsBlock => SpliceController.AttachedEffects(this).Any(e => e.GainsBlock) || base.GainsBlock;

    public override string BetaPortraitPath
    {
        get
        {
            var first = SpliceController.AttachedEffects(this).FirstOrDefault();
            return first != null ? first.BetaPortraitPath : base.BetaPortraitPath;
        }
    }

    /// <summary>目标类型由效果派生:拼了需要目标的效果(如攻击),打这张拼卡时就需要选目标。</summary>
    public override TargetType TargetType
    {
        get
        {
            foreach (var effect in SpliceController.AttachedEffects(this))
            {
                if (effect.TargetType is not (TargetType.None or TargetType.Self))
                {
                    return effect.TargetType;
                }
            }
            return base.TargetType;
        }
    }

    /// <summary>空壳默认无效果;子类可覆盖,给费用卡自身加一个基础效果。</summary>
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => Task.CompletedTask;

    /// <summary>
    /// 消耗后逐回合自动复播(原版 HowlFromBeyond 行为)。combat hook 只派发到战斗牌堆里的卡,
    /// 效果卡是 canonical 单例、不在这堆里,所以复播判定在宿主拼卡上做:
    /// 宿主在消耗牌堆、且拼了任一带 <see cref="EffectCardModelBase.ReplayWhenExhausted"/> 的效果时,
    /// 每回合结束后把整张拼卡重新 AutoPlay(重跑全部已拼效果)。
    /// </summary>
    public override async Task AfterAutoPostPlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner)
        {
            return;
        }
        if (Pile?.Type == PileType.Exhaust
            && SpliceController.AttachedEffects(this).Any(e => e.ReplayWhenExhausted))
        {
            await CardCmd.AutoPlay(choiceContext, this, null);
        }
    }

    /// <summary>
    /// 来源效果卡(按拼接顺序);未拼接返回 null。
    /// 数据本身已由 <see cref="EffectAttachmentModifier"/>(EffectCardId + JoinIndex)持久化,
    /// 这里提供只读便捷视图,满足"拼卡需记录来源效果卡(无则 null)"的需求。
    /// </summary>
    public IReadOnlyList<EffectCardModelBase>? SourceEffectCards
    {
        get
        {
            var effects = SpliceController.AttachedEffects(this).ToList();
            return effects.Count == 0 ? null : effects;
        }
    }
}
