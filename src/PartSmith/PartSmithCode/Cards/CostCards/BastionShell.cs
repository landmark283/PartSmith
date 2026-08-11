using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>费用卡:消耗 2 点能量,点数容量 15 的空壳(稀有)。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class BastionShell : CostCardModelBase
{
    public BastionShell() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    public override int PointCapacity => 15;
}
