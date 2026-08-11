#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>效果卡:点数 12。Fiend Fire。效果同原版 FiendFire。</summary>
[Pool(typeof(PartSmithEffectCardPool))]
public class FiendFireFragment : EffectCardModelBase
{
    public FiendFireFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 12;

    protected override CardModel PortraitSourceCard => ModelDb.Card<FiendFire>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(7m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Damage" => 3m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null) { return; }
        List<CardModel> list = PileType.Hand.GetPile(cardPlay.Player).Cards.ToList();
        int cardCount = list.Count;
        foreach (CardModel item in list)
        {
            await CardCmd.Exhaust(choiceContext, item);
        }
        float scale = 0.8f;
        await DamageCmd.Attack(UpgradedValue(cardPlay, base.DynamicVars.Damage.BaseValue, 3m)).WithHitCount(cardCount).FromCard(cardPlay.Card, cardPlay)
            .Targeting(cardPlay.Target)
            .BeforeDamage(delegate
            {
                NGroundFireVfx nGroundFireVfx = NGroundFireVfx.Create(cardPlay.Target);
                if (nGroundFireVfx == null)
                {
                    return Task.CompletedTask;
                }
                SfxCmd.Play("event:/sfx/characters/attack_fire");
                nGroundFireVfx.Scale = Vector2.One * scale;
                NCombatRoom.Instance?.CombatVfxContainer.AddChildSafely(nGroundFireVfx);
                scale += 0.1f;
                return Task.CompletedTask;
            })
            .Execute(choiceContext);
    }

}
