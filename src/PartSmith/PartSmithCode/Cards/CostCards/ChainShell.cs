using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>费用卡:1 费,点数容量 10(罕见)。无自身效果,链节拼合的中型框架。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class ChainShell : CostCardModelBase
{
    public ChainShell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    public override int PointCapacity => 10;
}
