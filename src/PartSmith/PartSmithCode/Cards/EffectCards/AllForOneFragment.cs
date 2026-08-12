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

/// <summary>效果卡:点数 12。All for One。效果同原版 AllForOne(机器人)。</summary>
[Pool(typeof(PartSmithRobotEffectCardPool))]
public class AllForOneFragment : EffectCardModelBase
{
    public AllForOneFragment() : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCost => 12;

    protected override CardModel PortraitSourceCard => ModelDb.Card<AllForOne>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new DamageVar(10m, ValueProp.Move),
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
            .WithHitFx("vfx/vfx_heavy_blunt", null, "blunt_attack.mp3")
            .WithHitVfxSpawnedAtBase()
            .Execute(choiceContext);
        // 原版 Filter 私有方法内联:0 费、非 X 费、类型为攻击/技能/能力(排除状态/诅咒)的弃牌堆卡全部入手。
        IEnumerable<CardModel> enumerable = PileType.Discard.GetPile(cardPlay.Player).Cards
            .Where((CardModel c) => c.EnergyCost.GetWithModifiers(CostModifiers.All) == 0 && !c.EnergyCost.CostsX
                && (uint)(c.Type - 1) <= 2u).ToList();
        foreach (CardModel item in enumerable)
        {
            await CardPileCmd.Add(item, PileType.Hand);
        }
    }

}
