using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>费用卡:0 费,点数容量 6(稀有)。无自身效果,免费的高容量大梁。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class Girder : CostCardModelBase
{
    public Girder() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    public override int PointCapacity => 6;
}
