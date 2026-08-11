using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>费用卡:消耗 2 点能量,点数容量 15(稀有)。自身效果:打出时失去 1 点生命。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class IronShell : CostCardModelBase
{
    public IronShell() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(choiceContext, base.Owner.Creature, 1m,
            ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
    }
}
