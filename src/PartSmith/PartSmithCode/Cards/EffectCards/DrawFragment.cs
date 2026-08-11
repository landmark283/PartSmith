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

/// <summary>效果卡示例:点数消耗 2,抽 2 张牌。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class DrawFragment : EffectCardModelBase
{
    public DrawFragment() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    public override int PointCost => 2;

    protected override CardModel? PortraitSourceCard => ModelDb.Card<BattleTrance>();

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 2m, cardPlay.Player);
    }
}
