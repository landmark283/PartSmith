using BaseLib.Utils;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 设计"公共池"17 张共享费用牌在骨头人池的瘦子类:只把 [Pool] 归属改为 PartSmithBoneManCostCardPool
/// (卡面费用图标/描边自动走 necrobinder 粉),效果/点数/稀有度/关键字全部继承战士池基类壳。
/// </summary>

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class TrinketShellBoneMan : TrinketShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class GemShellBoneMan : GemShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class BoardShellBoneMan : BoardShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class IngotShellBoneMan : IngotShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class TempDownShellBoneMan : TempDownShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class ExhaustShellBoneMan : ExhaustShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class FortShellBoneMan : FortShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class TempDownBigShellBoneMan : TempDownBigShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class KeepShellBoneMan : KeepShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class CraneShellBoneMan : CraneShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class VoidExhaustShellBoneMan : VoidExhaustShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class WeakenShellBoneMan : WeakenShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class SlowdownShellBoneMan : SlowdownShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class SlowdownBigShellBoneMan : SlowdownBigShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class DiscardShellBoneMan : DiscardShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class DazeShellBoneMan : DazeShell
{
}

[Pool(typeof(PartSmithBoneManCostCardPool))]
public class EmpowerShellBoneMan : EmpowerShell
{
}
