using BaseLib.Abstracts;

namespace PartSmith.PartSmithCode.Cards.Splicing;

/// <summary>
/// Melancholy 的"本场战斗死亡计数"暂存器:挂在宿主拼卡实例上,记录本场战斗里
/// 已有多少生物死亡(封顶 2),打出时按计数扣玩家的能量。
///
/// 死亡计数是**每场战斗独立**的:只在战斗实例的暂存器上递增(AfterDeath 派发到
/// 战斗牌堆里的宿主),卡组版本恒为 0,下一场战斗从卡组克隆出全新的 0 计数实例,
/// 与巨镰那种"跨战斗永久成长"相反,不需要同步回 DeckVersion。
/// </summary>
public class MelancholyExtraModifier : CardModifier
{
    /// <summary>本场战斗已死亡的生物数(0-2,封顶)。</summary>
    public int Deaths { get; set; }

    public override void StoreSaveData(ModifierSave save)
        => save.AdditionalProperties["Deaths"] = Deaths.ToString();

    public override void LoadSaveData(ModifierSave save)
    {
        save.AdditionalProperties.TryGetValue("Deaths", out string? s);
        Deaths = int.TryParse(s, out int v) ? v : 0;
    }
}
