using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡示例:点数消耗 3,对目标敌人造成 6 点伤害。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class StrikeFragment : EffectCardModelBase
{
    public StrikeFragment() : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 3;

    protected override CardModel? PortraitSourceCard => ModelDb.Card<StrikeIronclad>();

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }

        // 宿主卡 = cardPlay.Card(拼卡),攻击来源、攻击动画、力量加成都以它为准。
        await DamageCmd.Attack(6m)
            .FromCard(cardPlay.Card, cardPlay)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }
}
