using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>费用卡:1 费,点数容量 6(普通)。无自身效果,朴素的基础框架。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class Plank : CostCardModelBase
{
    public Plank() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    public override int PointCapacity => 6;
}
