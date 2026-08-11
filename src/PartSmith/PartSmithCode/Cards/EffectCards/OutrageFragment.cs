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
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 4。Outrage。效果同原版 Outrage。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class OutrageFragment : EffectCardModelBase
{
    public OutrageFragment() : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 4;

    protected override CardModel PortraitSourceCard => ModelDb.Card<Outrage>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(9m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 4m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 4m)).FromCard(cardPlay.Card, cardPlay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        if (cardPlay.Card.CombatState == null)
        {
            return;
        }
        IEnumerable<Creature> enumerable = from c in cardPlay.Card.CombatState.GetTeammatesOf(cardPlay.Player.Creature)
            where c != null && c.IsAlive && c.IsPlayer
            select c;
        foreach (Creature item in enumerable)
        {
            // 同 AngerFragment:克隆宿主拼卡而非效果卡单例(canonical 不可变会抛 CanonicalModelException)。
            CardModel card = cardPlay.Card.CreateCloneForPlayer(item.Player);
            CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, cardPlay.Player), 2.2f);
        }
    }

}
