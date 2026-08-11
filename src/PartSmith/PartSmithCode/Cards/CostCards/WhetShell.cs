using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>费用卡:1 费,点数容量 10(罕见)。自身效果:给选定的敌人增加 1 点力量(磨砺敌人)。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class WhetShell : CostCardModelBase
{
    public WhetShell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }
        await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, 1m, base.Owner.Creature, this);
    }
}
