using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 费用卡:消耗 1 点能量,点数容量 15 的空壳(稀有)。作为费用卡自带一个基础效果:
/// 打出时失去 3 点生命(抄 Bloodletting 的写法,Unblockable|Unpowered 保证必扣)。
/// </summary>
[Pool(typeof(PartSmithCostCardPool))]
public class BloodShell : CostCardModelBase
{
    public BloodShell() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(choiceContext, base.Owner.Creature, 3m,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
    }
}
