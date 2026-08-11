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
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Characters;

/// <summary>
/// 小猎人:猎人的 fork。阶段 A = 与原版猎人一模一样(纯副本)。
/// PlaceholderID="silent" → 视觉/动画/音效/能量计数器/角色选择界面全复用猎人资源(BaseLib 官方捷径)。
///
/// 数值完全照抄原版 Silent(反编译 MegaCrit.Sts2.Core.Models.Characters.Silent.cs):
/// StartingHp=70 / StartingGold=99 / 起始卡组 5×StrikeSilent+5×DefendSilent+Neutralize+Survivor /
/// 起始遗物 RingOfTheSnake / 卡池=SilentCardPool / 药水=SilentPotionPool / 遗物=SilentRelicPool。
/// 阶段 B 再改 CardPool 为 PartSmithHunterCostCardPool,并注入费用/效果双槽奖励。
/// </summary>
public class LittleHunter : PlaceholderCharacterModel
{
    public override string PlaceholderID => "silent";

    public override CharacterGender Gender => CharacterGender.Feminine;

    public override Color NameColor => StsColors.green;

    public override int StartingHp => 70;

    // 阶段 A 保持与原版猎人一致的卡池/药水/遗物(纯副本)。
    // 阶段 B3 曾把 CardPool 换成 PartSmithHunterCostCardPool,但商店(商人)按角色 CardPool 的
    // 卡牌类型+稀有度生成出售卡,而猎人池 14 个壳全是 Skill、无 Attack/Power → 商人要 Attack 卡时
    // 抛 "Can't generate valid rarity for merchant card type Attack" 崩溃。
    // 故 CardPool 保持原版 SilentCardPool(与大战士用 IroncladCardPool 同理):
    // 商店/事件给原版猎人卡,战斗奖励由 RewardInjectionPatch 注入猎人费用/效果池(拼卡体系不受影响)。
    public override CardPoolModel CardPool => ModelDb.CardPool<SilentCardPool>();

    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SilentPotionPool>();

    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SilentRelicPool>();

    public override IEnumerable<CardModel> StartingDeck => new CardModel[]
    {
        ModelDb.Card<StrikeSilent>(),
        ModelDb.Card<StrikeSilent>(),
        ModelDb.Card<StrikeSilent>(),
        ModelDb.Card<StrikeSilent>(),
        ModelDb.Card<StrikeSilent>(),
        ModelDb.Card<DefendSilent>(),
        ModelDb.Card<DefendSilent>(),
        ModelDb.Card<DefendSilent>(),
        ModelDb.Card<DefendSilent>(),
        ModelDb.Card<DefendSilent>(),
        ModelDb.Card<Neutralize>(),
        ModelDb.Card<Survivor>()
    };

    public override IReadOnlyList<RelicModel> StartingRelics => new[] { ModelDb.Relic<RingOfTheSnake>() };
}
