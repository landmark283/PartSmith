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

namespace PartSmith.PartSmithCode.Characters;

/// <summary>
/// 骨头人:亡灵契约师(Necrobinder)的 fork。阶段 A = 与原版亡灵契约师一模一样(纯副本)。
/// PlaceholderID="necrobinder" → 视觉/动画/音效/能量计数器/角色选择界面/Osty 骨狼全复用原版资源(BaseLib 官方捷径)。
///
/// 数值完全照抄原版 Necrobinder(反编译 MegaCrit.Sts2.Core.Models.Characters.Necrobinder.cs):
/// StartingHp=66 / StartingGold=99 / 起始卡组 4×StrikeNecrobinder+4×DefendNecrobinder+Bodyguard+Unleash /
/// 起始遗物 BoundPhylactery(战斗开始 + 每回合召唤 Osty +1HP) / 卡池=NecrobinderCardPool / 药水=NecrobinderPotionPool / 遗物=NecrobinderRelicPool。
///
/// Osty 骨狼宠物体系(召唤/治疗/复活宠物、宠物在场判定、宠物攻击、献祭宠物换收益)是基游戏原生机制,
/// PlaceholderID="necrobinder" 直接继承;且 Osty 相关命令(OstyCmd.Summon / Player.Osty / CreatureCmd.Kill)全部
/// 玩家无关(不检查角色类型),效果卡只需拿到 cardPlay.Player 即可操作宠物——比储君的星费方案 A 更简单。
///
/// 注意:
/// - 不覆写 GenerateAnimator 等动画方法(否则丢亡灵契约师动画),数值/配色照抄即可。
/// - 不覆写 UnlocksAfterRunAs(CustomCharacterModel 默认 null = 开局解锁,与 BigWarrior/LittleHunter/Wang 一致;
///   原版 Necrobinder 的 "打完 Regent 解锁" 链不保留,方便直接游玩)。
/// - 必须覆写 ExtraAssetPaths:原版 Necrobinder 用它对 Osty 的治疗 VFX 与骨狼视觉资源做预载
///   (vfx/vfx_heal_osty + creature_visuals/osty),PlaceholderCharacterModel 不提供,缺了 Osty 可能加载异常。
/// - 阶段 B3 再把奖励注入扩到骨头人;CardPool 保持原版 NecrobinderCardPool(商店/事件给原版亡灵契约师卡,
///   战斗奖励由 RewardInjectionPatch 注入骨头人费用/效果池)。
/// </summary>
public class BoneMan : PlaceholderCharacterModel
{
    public override string PlaceholderID => "necrobinder";

    public override CharacterGender Gender => CharacterGender.Feminine;

    public override Color NameColor => StsColors.purple;

    public override int StartingHp => 66;

    public override CardPoolModel CardPool => ModelDb.CardPool<NecrobinderCardPool>();

    public override PotionPoolModel PotionPool => ModelDb.PotionPool<NecrobinderPotionPool>();

    public override RelicPoolModel RelicPool => ModelDb.RelicPool<NecrobinderRelicPool>();

    public override IEnumerable<CardModel> StartingDeck => new CardModel[]
    {
        ModelDb.Card<StrikeNecrobinder>(),
        ModelDb.Card<StrikeNecrobinder>(),
        ModelDb.Card<StrikeNecrobinder>(),
        ModelDb.Card<StrikeNecrobinder>(),
        ModelDb.Card<DefendNecrobinder>(),
        ModelDb.Card<DefendNecrobinder>(),
        ModelDb.Card<DefendNecrobinder>(),
        ModelDb.Card<DefendNecrobinder>(),
        ModelDb.Card<Bodyguard>(),
        ModelDb.Card<Unleash>()
    };

    public override IReadOnlyList<RelicModel> StartingRelics => new[] { ModelDb.Relic<BoundPhylactery>() };

    // 原版 Necrobinder 用它对 Osty 的治疗 VFX 与骨狼视觉资源做预载(CharacterModel.AssetPaths 会拼上 ExtraAssetPaths)。
    protected override IEnumerable<string> ExtraAssetPaths => new string[]
    {
        SceneHelper.GetScenePath("vfx/vfx_heal_osty"),
        SceneHelper.GetScenePath("creature_visuals/osty")
    };

    // ---- 原版 Necrobinder 的数值/配色照抄(PlaceholderCharacterModel 未提供或与 Necrobinder 不同的部分)----

    public override float AttackAnimDelay => 0.15f;

    public override float CastAnimDelay => 0.4f;

    // 能量标签描边色(能量图标旁边的文字描边),原版 Necrobinder 专属。
    public override Color EnergyLabelOutlineColor => new Color("702D6FFF");

    public override Color DialogueColor => new Color("6B4658");

    public override Color MapDrawingColor => new Color("AC0486");

    public override Color RemoteTargetingLineColor => new Color("FD98C9FF");

    public override Color RemoteTargetingLineOutline => new Color("702D6FFF");

    // 原版 Necrobinder 用的是铁甲战士的转场音效(不是 necrobinder 专属)。
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    // 原版 Necrobinder 的守护者攻击 VFX 列表(挥打/钝击/斩击/血腥冲击)。
    public override List<string> GetArchitectAttackVfx() => new()
    {
        "vfx/vfx_thrash",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact"
    };
}
