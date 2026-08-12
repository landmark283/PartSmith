using BaseLib.Abstracts;

namespace PartSmith.PartSmithCode.Cards.Splicing;

/// <summary>
/// 爪击(Claw)的"本场战斗中每打一张爪击,所有爪击伤害 +N"成长暂存器:挂在每张带爪击效果的宿主拼卡实例上,
/// 记录这张爪击累积的额外伤害,并随存档持久化。
///
/// 为什么不用效果卡自己的 DynamicVars:效果卡是 ModelDb 里的共享单例,存数值会跨战斗、跨拼卡泄漏;
/// 挂在宿主卡实例上则每张爪击拼卡独立累积,且"所有爪击共享成长"通过 ClawFragment.ExecuteEffect
/// 遍历 PlayerCombatState.AllCards 里所有带爪击效果的卡并各自累加实现。
/// </summary>
public class ClawExtraModifier : CardModifier
{
    public decimal ExtraDamage { get; set; }

    public override void StoreSaveData(ModifierSave save)
        => save.AdditionalProperties["ExtraDamage"] = ExtraDamage.ToString();

    public override void LoadSaveData(ModifierSave save)
    {
        save.AdditionalProperties.TryGetValue("ExtraDamage", out string? s);
        ExtraDamage = decimal.TryParse(s, out decimal v) ? v : 0m;
    }
}
