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
/// 机器人:故障机器人(Defect)的 fork。阶段 A = 与原版故障机器人一模一样(纯副本)。
/// PlaceholderID="defect" → 视觉/动画/音效/能量计数器/角色选择界面/充能球栏位全复用故障机器人资源(BaseLib 官方捷径)。
///
/// 数值完全照抄原版 Defect(反编译 MegaCrit.Sts2.Core.Models.Characters.Defect.cs):
/// StartingHp=75 / StartingGold=99 / 起始卡组 4×StrikeDefect+4×DefendDefect+Zap+Dualcast /
/// 起始遗物 CrackedCore(战斗开始自动召唤 1 个闪电球) / 卡池=DefectCardPool / 药水=DefectPotionPool / 遗物=DefectRelicPool。
///
/// 充能球体系(球槽/充能/唤起/被动/球类型 Dark/Frost/Glass/Lightning/Plasma)是基游戏原生机制,
/// 关键就在 BaseOrbSlotCount:Player 构造时按 character.BaseOrbSlotCount 初始化球槽容量
/// (Player.cs:307 / PlayerCombatState.cs:139 OrbQueue.AddCapacity),OrbCmd.Channel 也用它判定是否自动加槽。
/// 缺了它机器人开局 0 槽 → 充能球牌全空转,必须覆写 = 3。
///
/// 注意:
/// - 不覆写 GenerateAnimator 等动画方法(否则丢故障机器人动画),数值/配色照抄即可。
/// - 不覆写 UnlocksAfterRunAs(CustomCharacterModel 默认 null = 开局解锁,与 BigWarrior/LittleHunter/Wang/BoneMan 一致;
///   原版 Defect 的 "打完亡灵契约师解锁" 链不保留,方便直接游玩)。
/// - 阶段 B3 再把奖励注入扩到机器人;CardPool 保持原版 DefectCardPool(商店/事件给原版故障机器人卡,
///   战斗奖励由 RewardInjectionPatch 注入机器人费用/效果池)。
/// </summary>
public class Robot : PlaceholderCharacterModel
{
    public override string PlaceholderID => "defect";

    public override CharacterGender Gender => CharacterGender.Neutral;

    public override Color NameColor => StsColors.blue;

    public override int StartingHp => 75;

    public override CardPoolModel CardPool => ModelDb.CardPool<DefectCardPool>();

    public override PotionPoolModel PotionPool => ModelDb.PotionPool<DefectPotionPool>();

    public override RelicPoolModel RelicPool => ModelDb.RelicPool<DefectRelicPool>();

    public override IEnumerable<CardModel> StartingDeck => new CardModel[]
    {
        ModelDb.Card<StrikeDefect>(),
        ModelDb.Card<StrikeDefect>(),
        ModelDb.Card<StrikeDefect>(),
        ModelDb.Card<StrikeDefect>(),
        ModelDb.Card<DefendDefect>(),
        ModelDb.Card<DefendDefect>(),
        ModelDb.Card<DefendDefect>(),
        ModelDb.Card<DefendDefect>(),
        ModelDb.Card<Zap>(),
        ModelDb.Card<Dualcast>()
    };

    public override IReadOnlyList<RelicModel> StartingRelics => new[] { ModelDb.Relic<CrackedCore>() };

    // ---- 原版 Defect 的数值/配色照抄(PlaceholderCharacterModel 未提供或与 Defect 不同的部分)----

    public override float AttackAnimDelay => 0.15f;

    public override float CastAnimDelay => 0.25f;

    public override float PowerUpAnimDelay => 0.5f;

    // 能量标签描边色,原版 Defect 专属。
    public override Color EnergyLabelOutlineColor => new Color("163E64FF");

    /// <summary>充能球栏位:原版 Defect = 3(球体系引擎,见类注释)。</summary>
    public override int BaseOrbSlotCount => 3;

    public override Color DialogueColor => new Color("13446B");

    public override VfxColor SpeechBubbleColor => VfxColor.Blue;

    public override Color MapDrawingColor => new Color("0D638C");

    public override Color RemoteTargetingLineColor => new Color("70B6EDFF");

    public override Color RemoteTargetingLineOutline => new Color("163E64FF");

    // 原版 Defect 用的是铁甲战士的转场音效(不是 defect 专属)。
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";

    // 原版 Defect 的守护者攻击 VFX 列表(闪电/钝击/抓挠/挥打/重击)。
    public override List<string> GetArchitectAttackVfx() => new()
    {
        "vfx/vfx_attack_lightning",
        "vfx/vfx_attack_blunt",
        "vfx/vfx_scratch",
        "vfx/vfx_attack_slash",
        "vfx/vfx_heavy_blunt"
    };
}
