using BaseLib.Utils;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 设计"公共池"17 张共享费用牌在机器人池的瘦子类:只把 [Pool] 归属改为 PartSmithRobotCostCardPool
/// (卡面费用图标/描边自动走 defect 蓝),效果/点数/稀有度/关键字全部继承战士池基类壳。
/// </summary>

[Pool(typeof(PartSmithRobotCostCardPool))]
public class TrinketShellRobot : TrinketShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class GemShellRobot : GemShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class BoardShellRobot : BoardShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class IngotShellRobot : IngotShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class TempDownShellRobot : TempDownShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class ExhaustShellRobot : ExhaustShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class FortShellRobot : FortShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class TempDownBigShellRobot : TempDownBigShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class KeepShellRobot : KeepShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class CraneShellRobot : CraneShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class VoidExhaustShellRobot : VoidExhaustShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class WeakenShellRobot : WeakenShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class SlowdownShellRobot : SlowdownShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class SlowdownBigShellRobot : SlowdownBigShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class DiscardShellRobot : DiscardShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class DazeShellRobot : DazeShell
{
}

[Pool(typeof(PartSmithRobotCostCardPool))]
public class EmpowerShellRobot : EmpowerShell
{
}
