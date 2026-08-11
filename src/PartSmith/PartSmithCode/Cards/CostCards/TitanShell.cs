using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>费用卡:3 费,点数容量 20(稀有)。无自身效果,沉重厚实的巨型框架。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class TitanShell : CostCardModelBase
{
    public TitanShell() : base(3, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    public override int PointCapacity => 20;
}
