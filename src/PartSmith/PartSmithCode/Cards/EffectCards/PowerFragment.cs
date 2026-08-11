using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>
/// 效果卡(能力):点数消耗 6,打出拼卡时获得 1 点力量(稀有)。
/// 类型标 Power,但拼接流程里它不会被单独打出,只是给拼卡提供效果脚本。
/// </summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class PowerFragment : EffectCardModelBase
{
    public PowerFragment() : base(0, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCost => 6;

    protected override CardModel? PortraitSourceCard => ModelDb.Card<Inflame>();

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Player.Creature, 1m, cardPlay.Player.Creature, cardPlay.Card);
    }
}
