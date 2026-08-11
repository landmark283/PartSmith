using BaseLib.Utils;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 设计"公共池"17 张共享费用牌在猎人池的瘦子类:只把 [Pool] 归属改为 PartSmithHunterCostCardPool
/// (卡面费用图标/描边自动走猎人 silent 绿),效果/点数/稀有度/关键字全部继承战士池基类壳。
/// 一张卡类只能归一个池([Pool] AllowMultiple=false),所以共享牌每角色都要瘦子类保配色。
/// </summary>

[Pool(typeof(PartSmithHunterCostCardPool))]
public class TrinketShellHunter : TrinketShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class GemShellHunter : GemShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class BoardShellHunter : BoardShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class IngotShellHunter : IngotShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class TempDownShellHunter : TempDownShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class ExhaustShellHunter : ExhaustShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class FortShellHunter : FortShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class TempDownBigShellHunter : TempDownBigShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class KeepShellHunter : KeepShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class CraneShellHunter : CraneShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class VoidExhaustShellHunter : VoidExhaustShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class WeakenShellHunter : WeakenShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class SlowdownShellHunter : SlowdownShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class SlowdownBigShellHunter : SlowdownBigShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class DiscardShellHunter : DiscardShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class DazeShellHunter : DazeShell
{
}

[Pool(typeof(PartSmithHunterCostCardPool))]
public class EmpowerShellHunter : EmpowerShell
{
}
