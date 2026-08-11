using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using PartSmith.PartSmithCode.Powers;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 费用卡:1 费,点数容量 15(罕见)。自身效果:本回合自己的力量-1、敏捷-1(回合结束自动恢复)。
/// 直接复用原版 TemporaryStrengthPower / TemporaryDexterityPower(IsPositive=false) 的机制:
/// 应用时立即扣,回合结束移除并补回,即"先立即-1,下回合+1"。
/// </summary>
[Pool(typeof(PartSmithCostCardPool))]
public class CrumblingShell : CostCardModelBase
{
    public CrumblingShell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<CrumblingShellStrengthDownPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
        await PowerCmd.Apply<CrumblingShellDexterityDownPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }
}
