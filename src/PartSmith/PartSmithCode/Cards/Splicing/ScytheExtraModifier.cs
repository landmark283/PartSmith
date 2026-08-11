using BaseLib.Abstracts;

namespace PartSmith.PartSmithCode.Cards.Splicing;

/// <summary>
/// 巨镰(TheScythe)的"永久成长伤害"暂存器:挂在宿主拼卡实例上,记录每次打出后
/// 累加的额外伤害,并随存档持久化(跨战斗保留,仿原版 <c>[SavedProperty] CurrentDamage</c>)。
///
/// 为什么不用效果卡自己的 DynamicVars:效果卡是 ModelDb 里的共享单例,把数值存在它身上
/// 会跨战斗、跨拼卡泄漏;挂在宿主卡实例上则每次打出的拼卡都是独立实例,互不干扰
/// (同 Rampage 的 <see cref="RampExtraModifier"/> 方案)。
///
/// 与原版 Rampage 的区别:巨镰的成长是"永久"(跨战斗),所以打出时要显式把成长值
/// 同步到卡组版本的暂存器(原版 TheScythe 用 <c>(DeckVersion as TheScythe)?.BuffFromPlay</c>)。
/// </summary>
public class ScytheExtraModifier : CardModifier
{
    public int ExtraDamage { get; set; }

    public override void StoreSaveData(ModifierSave save)
        => save.AdditionalProperties["ExtraDamage"] = ExtraDamage.ToString();

    public override void LoadSaveData(ModifierSave save)
    {
        save.AdditionalProperties.TryGetValue("ExtraDamage", out string? s);
        ExtraDamage = int.TryParse(s, out int v) ? v : 0;
    }
}
