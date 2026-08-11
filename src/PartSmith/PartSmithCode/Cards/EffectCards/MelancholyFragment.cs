#nullable disable
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Cards.Splicing;
using PartSmith.PartSmithCode.Pools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartSmith.PartSmithCode.Cards.EffectCards;

/// <summary>
/// 效果卡:点数 16。Melancholy。改版效果:每有 1 个生物死亡,打出此牌给玩家
/// +1 点能量(最多 +2)。**只加不扣**——死亡数越高这张牌打出来越赚(格挡 + 能量)。
///
/// 用户定版(多次更正后的最终语义):打出后**不扣任何费用,只给玩家加费用**,
/// 加的额度 = 本场战斗的死亡计数(最多 2)。原版是"每当任意生物死亡,此牌费用 -1"
/// (越打越便宜);改版反转为"死亡越多,打出后回馈能量越多"。
///
/// 实现:原版靠 <c>AfterDeath</c> 战斗钩子——效果卡(共享单例)收不到原生钩子,
/// 但挂载它的 <see cref="EffectAttachmentModifier"/> 已被 BaseLib 注册为战斗钩子
/// 订阅者,由修饰器把 AfterDeath 按宿主实例转发到 <see cref="OnHostAfterDeath"/>。
/// 死亡计数存宿主拼卡实例的 <see cref="MelancholyExtraModifier"/> 暂存器(0-2),
/// 每场战斗独立,不跨战斗保留(与巨镰的永久成长相反)。
/// </summary>
[Pool(typeof(PartSmithBoneManEffectCardPool))]
public class MelancholyFragment : EffectCardModelBase
{
    public MelancholyFragment() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCost => 16;

    public override bool GainsBlock => true;
    protected override CardModel PortraitSourceCard => ModelDb.Card<Melancholy>();
    protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
    {
        new BlockVar(13m, ValueProp.Move),
    };
    protected override decimal GetUpgradeDelta(string varName) => varName switch
    {
        "Block" => 4m,
        _ => 0m,
    };

    public override async Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 打出此牌:本场战斗每死一个生物,玩家 +1 能量(最多 2)。只加不扣——死亡数
        // 越高,这张牌打出来越赚(格挡 + 能量)。
        int x = Math.Min(FindExtra(cardPlay.Card)?.Deaths ?? 0, 2);
        if (x > 0)
        {
            await PlayerCmd.GainEnergy(x, cardPlay.Player);
        }
        await CreatureCmd.GainBlock(cardPlay.Player.Creature, UpgradedValue(cardPlay, base.DynamicVars.Block.BaseValue, 4m), base.DynamicVars.Block.Props, cardPlay);
    }

    /// <summary>
    /// 拼接时预创建死亡计数暂存器(战斗外安全;OnPlay 里不能 AddModifier,
    /// 见 <see cref="EffectCardModelBase.OnSplicedToHost"/> 说明)。
    /// </summary>
    public override void OnSplicedToHost(CardModel host)
        => EnsureExtra(host);

    /// <summary>
    /// 任意生物死亡 → 本场战斗死亡计数 +1(总上限 2)。只递增**当前战斗实例**
    /// 的暂存器,卡组版本保持 0,下一场战斗从卡组克隆出全新计数。
    /// </summary>
    public override Task OnHostAfterDeath(
        CardModel host, PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented)
    {
        if (!wasRemovalPrevented && FindExtra(host) is { } m && m.Deaths < 2)
        {
            m.Deaths++;
        }
        return Task.CompletedTask;
    }

    private static MelancholyExtraModifier FindExtra(CardModel card)
        => CardModifier.Modifiers(card).OfType<MelancholyExtraModifier>().FirstOrDefault();

    private static MelancholyExtraModifier EnsureExtra(CardModel card)
    {
        var m = FindExtra(card);
        if (m == null)
        {
            m = (MelancholyExtraModifier)CardModifier.Get<MelancholyExtraModifier>().MutableClone();
            CardModifier.AddModifier(card, m);
        }
        return m;
    }
}
