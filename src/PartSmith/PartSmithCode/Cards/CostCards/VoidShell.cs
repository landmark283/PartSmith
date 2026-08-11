using System.Collections.Generic;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>费用卡:3 费,点数容量 30(罕见)。虚无 + 消耗,当回合必须打出,否则消散。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class VoidShell : CostCardModelBase
{
    public VoidShell() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    public override int PointCapacity => 30;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal, CardKeyword.Exhaust };
}
