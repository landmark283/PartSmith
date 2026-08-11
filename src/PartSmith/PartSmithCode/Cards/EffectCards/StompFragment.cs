#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 3。Stomp(践踏,重做 2026-08-10;原 StampedeFragment 改名)。
/// 对所有敌人造成 base 伤害(0,升级后 4),且本回合每打出一张攻击牌,此卡造成的伤害增加 4。
/// 攻击计数只数 CardPlaysFinished 里已完成的攻击(正在打出的拼卡本身不计,与原版 Finisher 相同)。
/// 卡图来源基础游戏 Stomp(践踏)。注意:基础游戏里 Stampede 类是"惊逃"(2费能力),不是本卡。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class StompFragment : EffectCardModelBase
{
    public StompFragment() : base(0, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
    {
    }

    public override int PointCost => 3;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Stomp>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new CalculationBaseVar(0m),
        new ExtraDamageVar(4m),
        new CalculatedDamageVar(ValueProp.Move).WithMultiplier((CardModel card, Creature _) =>
            card.Owner != null
                ? CombatManager.Instance.History.CardPlaysFinished.Count(e =>
                    e.HappenedThisTurn(card.CombatState)
                    && e.CardPlay.Card.Type == CardType.Attack
                    && e.CardPlay.Player == card.Owner)
                : 0m),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "CalculatedDamage" => 4m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 预览里的 CalculatedDamage 只显示基础值(0/4):Calculate 用 canonical 效果卡的
        // CombatState(null)→ 倍数恒 0(见 proposal_effect_card_dynamic_damage.md)。实际伤害在这里按宿主上下文算。
        int attacksPlayed = CombatManager.Instance.History.CardPlaysFinished.Count(e =>
            e.HappenedThisTurn(cardPlay.Card.CombatState)
            && e.CardPlay.Card.Type == CardType.Attack
            && e.CardPlay.Player == cardPlay.Player
            && e.CardPlay != cardPlay); // 排除正在打出的拼卡本身(ExecuteEffect 运行时它已在 CardPlaysFinished 里)
        decimal damage = UpgradedValue(cardPlay, base.DynamicVars.CalculationBase.BaseValue, 4m)
                         + base.DynamicVars.ExtraDamage.BaseValue * attacksPlayed;
        await DamageCmd.Attack(damage).FromCard(cardPlay.Card, cardPlay).TargetingAllOpponents(cardPlay.Card.CombatState)
            .WithHitFx("vfx/vfx_heavy_blunt", null, "heavy_attack.mp3")
            .Execute(choiceContext);
    }

}
