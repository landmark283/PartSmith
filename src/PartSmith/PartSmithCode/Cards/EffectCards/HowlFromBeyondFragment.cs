#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 16。Howl From Beyond。效果同原版 HowlFromBeyond(含消耗后逐回合自动复播)。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class HowlFromBeyondFragment : EffectCardModelBase
{
    public HowlFromBeyondFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 16;

    /// <summary>Exhaust 关键词:拼接时由 SpliceController 转移到宿主,拼卡打出后进消耗牌堆。</summary>
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

    /// <summary>消耗后逐回合自动复播(复播判定在宿主 CostCardModelBase.AfterAutoPostPlayPhaseEntered)。</summary>
    public override bool ReplayWhenExhausted => true;

    protected override CardModel PortraitSourceCard => ModelDb.Card<HowlFromBeyond>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(18m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 6m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 6m)).FromCard(cardPlay.Card, cardPlay).TargetingAllOpponents(cardPlay.Card.CombatState)
            .WithAttackerAnim("Cast", cardPlay.Player.Character.CastAnimDelay)
            .WithHitFx("vfx/vfx_attack_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

}
