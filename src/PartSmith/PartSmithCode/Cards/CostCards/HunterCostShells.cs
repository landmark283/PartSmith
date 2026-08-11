using BaseLib.Utils;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 小猎人费用卡壳:14 个轻量子类,复用战士壳全部逻辑(能量费/类型/稀有度/点数容量/自身效果/关键字),
/// 仅把 [Pool] 归属改为 PartSmithHunterCostCardPool → 卡面费用图标/描边自动走猎人 silent 绿,
/// 战士壳继续留在 PartSmithCostCardPool(ironclad),两套互不干扰。
///
/// 为什么需要 14 个子类而不是共用同一张卡:
/// BaseLib 的 [Pool] 是 AllowMultiple=false,且 CardModel._pool 按"包含该卡的首个池"解析(CardModel.Pool),
/// 一张卡类只能归一个池;战士壳已注册进 PartSmithCostCardPool,不能再进猎人池。
/// 子类声明自己的 [Pool] 会覆写继承的([Pool] Inherited=true + AllowMultiple=false 时,派生声明优先,
/// CustomContentDictionary.AddModel 用 GetCustomAttribute 读取即得到子类的池)。
/// 子类无需写构造函数(C# 默认调 base() 无参构造),被 ModelDb.AllAbstractModelSubtypes 反射实例化时自动注册。
/// </summary>

/// <summary>费用卡:0 费,点数容量 3(普通)。无自身效果,最廉价的便宜小框架。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class ScrapHunter : Scrap
{
}

/// <summary>费用卡:0 费,点数容量 6(稀有)。无自身效果,免费的高容量大梁。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class GirderHunter : Girder
{
}

/// <summary>费用卡:0 费,点数容量 6(罕见)。自身效果:打出时失去 1 点生命(锈壳割手)。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class RustShellHunter : RustShell
{
}

/// <summary>费用卡:1 费,点数容量 6(普通)。无自身效果,朴素的基础框架。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class PlankHunter : Plank
{
}

/// <summary>费用卡:1 费,点数容量 10(罕见)。无自身效果,链节拼合的中型框架。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class ChainShellHunter : ChainShell
{
}

/// <summary>费用卡:1 费,点数容量 10(罕见)。自身效果:给选定的敌人增加 1 点力量(磨砺敌人)。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class WhetShellHunter : WhetShell
{
}

/// <summary>费用卡:1 费,点数容量 15(罕见)。自身效果:本回合自己的力量-1、敏捷-1(回合结束自动恢复)。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class CrumblingShellHunter : CrumblingShell
{
}

/// <summary>费用卡:2 费,点数容量 20(罕见)。自身效果:永久失去 1 点力量(负重压身)。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class DeadweightShellHunter : DeadweightShell
{
}

/// <summary>费用卡:3 费,点数容量 20(稀有)。无自身效果,沉重厚实的巨型框架。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class TitanShellHunter : TitanShell
{
}

/// <summary>费用卡:3 费,点数容量 30(罕见)。虚无 + 消耗,当回合必须打出,否则消散。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class VoidShellHunter : VoidShell
{
}

/// <summary>费用卡:2 费,点数容量 15(普通)。消耗:打出后即弃,不再进弃牌堆。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class SocketedShellHunter : SocketedShell
{
}

/// <summary>费用卡:2 费,点数容量 15(稀有)。无自身效果,结实的堡垒框架。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class BastionShellHunter : BastionShell
{
}

/// <summary>费用卡:1 费,点数容量 15(稀有)。自身效果:打出时失去 3 点生命。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class BloodShellHunter : BloodShell
{
}

/// <summary>费用卡:2 费,点数容量 15(稀有)。自身效果:打出时失去 1 点生命。猎人版。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class IronShellHunter : IronShell
{
}
