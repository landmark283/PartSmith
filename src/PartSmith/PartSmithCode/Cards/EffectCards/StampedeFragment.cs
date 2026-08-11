#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 11。Stampede(惊逃,移植重做 2026-08-10)。
/// 效果尽量贴近原版:打出时施加 StampedePower(回合结束阶段随机自动打出 N 张手牌攻击,可叠层)。
/// 原版升级为费用 -1 → 按 §9.2 惯例改为升级时本回合获得 1 点能量(UpgradeEnergyGain=1)。
/// 已核实:StampedePower 走 CardCmd.AutoPlay(card.OnPlayWrapper),可正常打出拼接攻击牌
/// (幽冥嚎叫自动复播同一 API 实测通过);拼卡 Type=Attack 能过其 Type==Attack 过滤。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class StampedeFragment : EffectCardModelBase
{
    public StampedeFragment() : base(0, CardType.Power, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 11;

    public override int UpgradeEnergyGain => 1;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Stampede>();

    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new PowerVar<StampedePower>(1m),
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(cardPlay.Player.Creature, "PowerUp", cardPlay.Player.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<StampedePower>(choiceContext, cardPlay.Player.Creature, base.DynamicVars["StampedePower"].BaseValue, cardPlay.Player.Creature, cardPlay.Card);
    }

}
