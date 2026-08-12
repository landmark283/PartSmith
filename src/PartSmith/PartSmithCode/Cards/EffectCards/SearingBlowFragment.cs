#nullable disable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>
/// 效果卡:点数 6。Searing Blow(灼热打击)。自制经典老牌,原版 StS2 无此卡(卡面临时借 AshenStrike)。
/// 核心机制 = 可无限次升级,每次升级伤害增加递增(+4,+5,+6,…):
/// damage(L) = 12 + 3L + L(L+1)/2(0级12,1级16,2级21,3级27,4级34…)。
/// </summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class SearingBlowFragment : EffectCardModelBase
{
    public SearingBlowFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 6;

    /// <summary>吃满宿主所有升级级数(无限成长)。</summary>
    public override int MaxUpgradeLevels => int.MaxValue;

    /// <summary>原版无 SearingBlow,临时借 AshenStrike(燃烧主题打击)卡面,待自定义卡图任务替换。</summary>
    protected override CardModel PortraitSourceCard => ModelDb.Card<AshenStrike>();

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(12m, ValueProp.Move),
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        int dmg = DamageAt(EffectiveUpgradeLevels(cardPlay.Card));
        await DamageCmd.Attack(dmg).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    /// <summary>伤害随升级级数非线性成长:damage(L)=12+3L+L(L+1)/2(每级增量递增 +4,+5,+6…)。</summary>
    private static int DamageAt(int levels)
    {
        long l = levels;
        long dmg = 12 + 3 * l + l * (l + 1) / 2;
        return (int)Math.Min(dmg, int.MaxValue);
    }

    /// <summary>预览:显示当前级数真实伤害(+力量/易伤等全局钩子);diff 基线=当前伤害,成长量不重复绿。</summary>
    public override void RefreshPreviewForHost(CardModel hostCard, Creature target)
    {
        bool runGlobalHooks = hostCard.CombatState != null
            && (hostCard.Pile?.Type is PileType.Hand or PileType.Play
                || hostCard.UpgradePreviewType == CardUpgradePreviewType.Combat);
        int levels = EffectiveUpgradeLevels(hostCard);
        var dmg = base.DynamicVars["Damage"];
        dmg.PreviewValue = dmg.BaseValue;
        dmg.EnchantedValue = dmg.BaseValue;
        dmg.UpdateCardPreview(hostCard, CardPreviewMode.Normal, target, runGlobalHooks);
        decimal hooks = dmg.PreviewValue - dmg.BaseValue;
        dmg.PreviewValue = DamageAt(levels) + hooks;
        dmg.EnchantedValue = DamageAt(levels);
    }

}
