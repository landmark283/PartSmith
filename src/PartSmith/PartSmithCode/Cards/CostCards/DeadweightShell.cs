using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>费用卡:2 费,点数容量 20(罕见)。自身效果:永久失去 1 点力量(负重压身)。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class DeadweightShell : CostCardModelBase
{
    public DeadweightShell() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 20;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, -1m, base.Owner.Creature, this);
    }
}
