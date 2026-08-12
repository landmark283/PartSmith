using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using PartSmith.PartSmithCode.Powers;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 机器人(故障机器人)专属 8 张费用牌,只进 PartSmithRobotCostCardPool。
/// 充能球 = 机器人的核心机制(BaseOrbSlotCount=3,OrbCmd.Channel/EvokeNext/AddSlots/RemoveSlots 原生 API)。
/// 公共池 17 张共享壳的机器人瘦子类见 RobotSharedCostShells.cs。
/// </summary>

/// <summary>机器人专属 #1:0 费,点数容量 6(罕见)。自身效果:向手牌中增加一张伤口。</summary>
[Pool(typeof(PartSmithRobotCostCardPool))]
public class WoundInHandShell : CostCardModelBase
{
    public WoundInHandShell() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 6;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel card = base.CombatState!.CreateCard<Wound>(base.Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, base.Owner));
    }
}

/// <summary>机器人专属 #2:0 费,点数容量 6(罕见)。自身效果:本回合力量-2、敏捷-2(回合结束自动恢复)。</summary>
[Pool(typeof(PartSmithRobotCostCardPool))]
public class TempDown2Shell : CostCardModelBase
{
    public TempDown2Shell() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 6;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<TempStrengthDownPower>(choiceContext, base.Owner.Creature, 2m, base.Owner.Creature, this);
        await PowerCmd.Apply<TempDexterityDownPower>(choiceContext, base.Owner.Creature, 2m, base.Owner.Creature, this);
    }
}

/// <summary>机器人专属 #3:0 费,点数容量 3(罕见)。自身效果:触发一个充能球(唤起 = 触发 + 移除队首球)。</summary>
[Pool(typeof(PartSmithRobotCostCardPool))]
public class EvokeOrbShell : CostCardModelBase
{
    public EvokeOrbShell() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 3;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await OrbCmd.EvokeNext(choiceContext, base.Owner);
    }
}

/// <summary>机器人专属 #4:2 费,点数容量 12(稀有)。自身效果:这张牌打出两次(ModifyCardPlayCount 同 DuplicationPower)。</summary>
[Pool(typeof(PartSmithRobotCostCardPool))]
public class DoublePlayShell : CostCardModelBase
{
    public DoublePlayShell() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 12;

    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
        => card == this ? playCount + 1 : playCount;
}

/// <summary>机器人专属 #5:1 费,点数容量 10(普通)。自身效果:向抽牌堆中加入一张眩晕。</summary>
[Pool(typeof(PartSmithRobotCostCardPool))]
public class DazedToDrawShell : CostCardModelBase
{
    public DazedToDrawShell() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel card = base.CombatState!.CreateCard<Dazed>(base.Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Draw, base.Owner));
    }
}

/// <summary>机器人专属 #6:1 费,点数容量 15(稀有)。自身效果:充能球栏位减少 1(从队尾删)。</summary>
[Pool(typeof(PartSmithRobotCostCardPool))]
public class OrbSlotDownShell : CostCardModelBase
{
    public OrbSlotDownShell() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        OrbCmd.RemoveSlots(base.Owner, 1);
        await Task.CompletedTask;
    }
}

/// <summary>机器人专属 #7:3 费,点数容量 30(普通)。自身效果:力量-2(永久),充能球栏位增加 1。</summary>
[Pool(typeof(PartSmithRobotCostCardPool))]
public class OrbSlotUpShell : CostCardModelBase
{
    public OrbSlotUpShell() : base(3, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCapacity => 30;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, -2m, base.Owner.Creature, this);
        await OrbCmd.AddSlots(base.Owner, 1);
    }
}

/// <summary>机器人专属 #8:0 费,点数容量 4(罕见),无自身效果。</summary>
[Pool(typeof(PartSmithRobotCostCardPool))]
public class ChipShell : CostCardModelBase
{
    public ChipShell() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 4;
}
