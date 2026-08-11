using BaseLib.Utils;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 设计"公共池"17 张共享费用牌在王池的瘦子类:只把 [Pool] 归属改为 PartSmithWangCostCardPool
/// (卡面费用图标/描边自动走储君 regent 橙),效果/点数/稀有度/关键字全部继承战士池基类壳。
/// </summary>

[Pool(typeof(PartSmithWangCostCardPool))]
public class TrinketShellWang : TrinketShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class GemShellWang : GemShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class BoardShellWang : BoardShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class IngotShellWang : IngotShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class TempDownShellWang : TempDownShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class ExhaustShellWang : ExhaustShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class FortShellWang : FortShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class TempDownBigShellWang : TempDownBigShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class KeepShellWang : KeepShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class CraneShellWang : CraneShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class VoidExhaustShellWang : VoidExhaustShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class WeakenShellWang : WeakenShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class SlowdownShellWang : SlowdownShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class SlowdownBigShellWang : SlowdownBigShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class DiscardShellWang : DiscardShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class DazeShellWang : DazeShell
{
}

[Pool(typeof(PartSmithWangCostCardPool))]
public class EmpowerShellWang : EmpowerShell
{
}
