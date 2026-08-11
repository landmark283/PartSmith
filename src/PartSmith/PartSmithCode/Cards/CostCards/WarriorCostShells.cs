using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 战士(铁甲战士)专属 7 张费用牌,只进 PartSmithCostCardPool。
/// 公共池 17 张共享壳见 SharedCostShells(各角色瘦子类见 *SharedCostShells.cs)。
/// </summary>

/// <summary>战士专属 #1:0 费,点数容量 6(罕见)。自身效果:失去 1 点生命。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class LifeCostShell : CostCardModelBase
{
    public LifeCostShell() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 6;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(choiceContext, base.Owner.Creature, 1m, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
    }
}

/// <summary>战士专属 #2:1 费,点数容量 10(罕见)。自身效果:失去 1 点生命。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class LifeCostBigShell : CostCardModelBase
{
    public LifeCostBigShell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.Damage(choiceContext, base.Owner.Creature, 1m, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
    }
}

/// <summary>战士专属 #3:2 费,点数容量 15(普通)。自身效果:力量-1(永久)。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class StrengthCostShell : CostCardModelBase
{
    public StrengthCostShell() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, -1m, base.Owner.Creature, this);
    }
}

/// <summary>战士专属 #4:2 费,点数容量 10(罕见)。自身效果:失去 1 点生命,重复 3 次。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class ThriceLifeCostShell : CostCardModelBase
{
    public ThriceLifeCostShell() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        for (int i = 0; i < 3; i++)
        {
            await CreatureCmd.Damage(choiceContext, base.Owner.Creature, 1m, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
        }
    }
}

/// <summary>战士专属 #5:1 费,点数容量 12(稀有)。自身效果:给选定的敌人施加 2 层人工制品。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class ArtifactGiftShell : CostCardModelBase
{
    public ArtifactGiftShell() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCapacity => 12;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }
        await PowerCmd.Apply<ArtifactPower>(choiceContext, cardPlay.Target, 2m, base.Owner.Creature, this);
    }
}

/// <summary>战士专属 #6:0 费,点数容量 10(稀有)。自身效果:力量-1、失去 1 点生命。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class DualCostShell : CostCardModelBase
{
    public DualCostShell() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, -1m, base.Owner.Creature, this);
        await CreatureCmd.Damage(choiceContext, base.Owner.Creature, 1m, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
    }
}

/// <summary>战士专属 #7:3 费,点数容量 30(罕见)。自身效果:力量-2(永久)。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class StrengthCostHeavyShell : CostCardModelBase
{
    public StrengthCostHeavyShell() : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 30;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, -2m, base.Owner.Creature, this);
    }
}
