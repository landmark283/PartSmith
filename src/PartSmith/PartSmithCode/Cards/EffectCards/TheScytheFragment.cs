#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Cards.Splicing;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>
/// 效果卡:点数 12。The Scythe。效果同原版 TheScythe(亡灵契约师)。
///
/// 原版的"永久成长"(<c>[SavedProperty] CurrentDamage</c> 跨战斗/跨存档持久)在效果卡
/// (共享单例)上无法承载,所以成长状态存在宿主拼卡实例的 <see cref="ScytheExtraModifier"/>
/// 暂存器上(同 Rampage 的 RampExtraModifier 方案);与 Rampage 不同的是本卡成长是永久的,
/// 所以每次打出要把成长值同步到卡组版本(<see cref="CardModel.DeckVersion"/>),跨战斗保留
/// (仿原版 <c>(DeckVersion as TheScythe)?.BuffFromPlay</c>)。
/// </summary>
[Pool(typeof(PartSmithBoneManEffectCardPool))]
public class TheScytheFragment : EffectCardModelBase
{
    public TheScytheFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 12;

    protected override CardModel PortraitSourceCard => ModelDb.Card<TheScythe>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(13m, ValueProp.Move),
        new DynamicVar("Increase", 5m),
    };

    /// <summary>升级只提高"每次打出的成长增量"(Increase 5→7),不提高当前伤害(成长是状态不是升级)。</summary>
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Increase" => 2m,
        _ => 0m,
    };

    /// <summary>
    /// 拼接时预创建成长暂存器(战斗外安全):OnPlay 里 BaseLib 正在 foreach 卡的 modifiers,
    /// 不能 AddModifier,所以暂存器在拼接时建好,打出时只写值。
    /// </summary>
    public override void OnSplicedToHost(CardModel host)
        => EnsureExtra(host);

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        var growth = FindExtra(cardPlay.Card);
        int extra = growth?.ExtraDamage ?? 0;
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue + extra).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 每次打出永久成长:战斗实例 + 卡组版本都写(卡组版本负责跨战斗/跨存档保留)。
        // 只在暂存器已存在时写(拼接时已建;旧档/异常路径没有则跳过,不在此 AddModifier)。
        int inc = (int)UpgradedValue(cardPlay, base.DynamicVars["Increase"].BaseValue, 2m);
        int grown = extra + inc;
        if (growth != null)
        {
            growth.ExtraDamage = grown;
        }
        if (cardPlay.Card.DeckVersion is { } deck && FindExtra(deck) is { } deckGrowth)
        {
            deckGrowth.ExtraDamage = grown;
        }
    }

    /// <summary>
    /// 预览展示"成长后的伤害":效果卡 Damage 基础值 13 是共享单例(固定),实际伤害 =
    /// 13 + 宿主暂存器里的成长值。覆写基类预览,让手牌里的拼卡显示当前真实伤害
    /// (同 Rampage 的处理思路,只是 Rampage 未覆写、这里覆写因为巨镰的成长是核心卖点)。
    /// </summary>
    public override void RefreshPreviewForHost(CardModel hostCard, Creature target)
    {
        bool runGlobalHooks = hostCard.CombatState != null
            && (hostCard.Pile?.Type is PileType.Hand or PileType.Play
                || hostCard.UpgradePreviewType == CardUpgradePreviewType.Combat);
        int levels = EffectiveUpgradeLevels(hostCard);
        decimal grown = FindExtra(hostCard)?.ExtraDamage ?? 0m;
        foreach (var v in DynamicVars.Values)
        {
            decimal delta = GetUpgradeDelta(v.Name) * levels;
            v.PreviewValue = v.BaseValue;
            if (v.Name == "Damage")
            {
                // 成长不是升级增量、也不是附魔:以基础值(13)作 diff 比较基线,
                // 成长量在 UpdateCardPreview 之后叠加——它从 BaseValue 重算并整体覆盖
                // PreviewValue,预先加会被冲掉(修复"描述写死 13、实际打出 18")。
                delta = 0m;
                v.EnchantedValue = v.BaseValue;
            }
            if (delta != 0m)
            {
                v.EnchantedValue = v.BaseValue + delta;
            }
            v.UpdateCardPreview(hostCard, CardPreviewMode.Normal, target, runGlobalHooks);
            if (v.Name == "Damage")
            {
                v.PreviewValue += grown;
            }
            if (delta != 0m)
            {
                v.PreviewValue += delta;
            }
        }
    }

    private static ScytheExtraModifier FindExtra(CardModel card)
        => CardModifier.Modifiers(card).OfType<ScytheExtraModifier>().FirstOrDefault();

    private static ScytheExtraModifier EnsureExtra(CardModel card)
    {
        var m = FindExtra(card);
        if (m == null)
        {
            m = (ScytheExtraModifier)CardModifier.Get<ScytheExtraModifier>().MutableClone();
            CardModifier.AddModifier(card, m);
        }
        return m;
    }
}
