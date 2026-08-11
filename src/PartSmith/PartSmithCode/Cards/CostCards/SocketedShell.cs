using System.Collections.Generic;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>费用卡:2 费,点数容量 15(普通)。消耗:打出后即弃,不再进弃牌堆。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class SocketedShell : CostCardModelBase
{
    public SocketedShell() : base(2, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    public override int PointCapacity => 15;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
}
