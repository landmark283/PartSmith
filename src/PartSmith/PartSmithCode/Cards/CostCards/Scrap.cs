using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>费用卡:0 费,点数容量 3(普通)。无自身效果,最廉价的便宜小框架。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class Scrap : CostCardModelBase
{
    public Scrap() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    public override int PointCapacity => 3;
}
