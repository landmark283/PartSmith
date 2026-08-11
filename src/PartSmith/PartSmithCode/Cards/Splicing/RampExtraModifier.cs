using BaseLib.Abstracts;

namespace PartSmith.PartSmithCode.Cards.Splicing;

/// <summary>
/// 追击/连打类效果(Rampage / Thrash)的"额外伤害"暂存器:挂在宿主拼卡实例上,
/// 记录每次打出后累加的额外伤害,并随存档持久化。
///
/// 为什么不用效果卡自己的 DynamicVars:效果卡是 ModelDb 里的共享单例
/// (ResolveEffectCard → ModelDb.GetById 返回同一个实例),把数值存在它身上会跨战斗、
/// 跨拼卡泄漏;挂在宿主卡实例上则每次打出的拼卡都是独立实例,互不干扰。
/// </summary>
public class RampExtraModifier : CardModifier
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
