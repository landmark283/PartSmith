using BaseLib.Abstracts;

namespace PartSmith.PartSmithCode.Cards.Splicing;

/// <summary>
/// 遗传算法(GeneticAlgorithm)的"每打出一次,这张牌格挡永久 +N"成长暂存器:挂在宿主拼卡实例上,
/// 记录累积的额外格挡,并随存档持久化。
///
/// 为什么不用效果卡自己的 DynamicVars:效果卡是 ModelDb 里的共享单例,存数值会跨战斗、跨拼卡泄漏;
/// 挂在宿主卡实例上则每次打出的拼卡独立累积。GeneticAlgorithmFragment.ExecuteEffect 同时更新
/// 宿主战斗实例与宿主牌组版本(cardPlay.Card.DeckVersion)的暂存器,实现"本局游戏永久成长"。
/// </summary>
public class GenAlgoExtraModifier : CardModifier
{
    public int IncreasedBlock { get; set; }

    public override void StoreSaveData(ModifierSave save)
        => save.AdditionalProperties["IncreasedBlock"] = IncreasedBlock.ToString();

    public override void LoadSaveData(ModifierSave save)
    {
        save.AdditionalProperties.TryGetValue("IncreasedBlock", out string? s);
        IncreasedBlock = int.TryParse(s, out int v) ? v : 0;
    }
}
