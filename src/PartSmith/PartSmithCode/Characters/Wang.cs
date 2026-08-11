using System.Collections.Generic;
using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace PartSmith.PartSmithCode.Characters;

/// <summary>
/// 王:储君(Regent)的 fork。阶段 A = 与原版储君一模一样(纯副本)。
/// PlaceholderID="regent" → 视觉/动画/音效/能量计数器/角色选择界面/辉星计数器全复用储君资源(BaseLib 官方捷径)。
///
/// 数值完全照抄原版 Regent(反编译 MegaCrit.Sts2.Core.Models.Characters.Regent.cs):
/// StartingHp=75 / StartingGold=99 / 起始卡组 4×StrikeRegent+4×DefendRegent+FallingStar+Venerate /
/// 起始遗物 DivineRight(每战斗房 +3 辉星) / 卡池=RegentCardPool / 药水=RegentPotionPool / 遗物=RegentRelicPool。
///
/// 双资源体系(能量 + 跨回合累积的辉星)是基游戏原生机制,PlaceholderID="regent" 直接继承:
/// ShouldAlwaysShowStarCounter=true → 辉星计数器常显;星费卡灰显/扣星、铸造/君王之剑/仆从 Token 全原生可用。
///
/// 注意:
/// - 不覆写 GenerateAnimator 等动画方法(否则丢储君动画),数值/配色照抄即可。
/// - 不覆写 UnlocksAfterRunAs(CustomCharacterModel 默认 null = 开局解锁,与 BigWarrior/LittleHunter 一致;
///   原版 Regent 的 "打完 Silent 解锁" 链不保留,方便直接游玩)。
/// - 阶段 B3 再把奖励注入扩到王;CardPool 保持原版 RegentCardPool(商店/事件给原版储君卡,
///   战斗奖励由 RewardInjectionPatch 注入王费用/效果池)。
/// </summary>
public class Wang : PlaceholderCharacterModel
{
    public override string PlaceholderID => "regent";

    public override CharacterGender Gender => CharacterGender.Masculine;

    public override Color NameColor => StsColors.orange;

    public override int StartingHp => 75;

    // 储君专有覆盖:始终显示辉星计数器(星体系 UI)。
    public override bool ShouldAlwaysShowStarCounter => true;

    public override CardPoolModel CardPool => ModelDb.CardPool<RegentCardPool>();

    public override PotionPoolModel PotionPool => ModelDb.PotionPool<RegentPotionPool>();

    public override RelicPoolModel RelicPool => ModelDb.RelicPool<RegentRelicPool>();

    public override IEnumerable<CardModel> StartingDeck => new CardModel[]
    {
        ModelDb.Card<StrikeRegent>(),
        ModelDb.Card<StrikeRegent>(),
        ModelDb.Card<StrikeRegent>(),
        ModelDb.Card<StrikeRegent>(),
        ModelDb.Card<DefendRegent>(),
        ModelDb.Card<DefendRegent>(),
        ModelDb.Card<DefendRegent>(),
        ModelDb.Card<DefendRegent>(),
        ModelDb.Card<FallingStar>(),
        ModelDb.Card<Venerate>()
    };

    public override IReadOnlyList<RelicModel> StartingRelics => new[] { ModelDb.Relic<DivineRight>() };

    // ---- 原版 Regent 的数值/配色照抄(PlaceholderCharacterModel 未提供或与 Regent 不同的部分)----

    // 星费标签描边色(辉星图标旁边的文字描边),原版 Regent 专属。
    public override Color EnergyLabelOutlineColor => new Color("784000FF");

    public override Color DialogueColor => new Color("52371D");

    public override VfxColor SpeechBubbleColor => VfxColor.Orange;

    public override Color MapDrawingColor => new Color("935206");

    public override Color RemoteTargetingLineColor => new Color("BFA270FF");

    public override Color RemoteTargetingLineOutline => new Color("784000FF");

    // 原版 Regent 用的是铁甲战士的转场音效(不是 regent 专属)。
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    // 原版 Regent 的守护者攻击 VFX 列表(星光/钝击/斩击/重击/闪电)。
    public override List<string> GetArchitectAttackVfx() => new()
    {
        "vfx/vfx_starry_impact",
        "vfx/vfx_attack_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_lightning"
    };
}
