using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 王(储君)专属 7 张费用牌,只进 PartSmithWangCostCardPool。
/// 星辉 = 王独有的双费用机制第二个费用(PlayerCmd.GainStars/LoseStars)。
/// 公共池 17 张共享壳的王瘦子类见 WangSharedCostShells.cs。
/// </summary>

/// <summary>王专属 #1:0 费,点数容量 3(稀有)。自身效果:获得 2 星辉。</summary>
[Pool(typeof(PartSmithWangCostCardPool))]
public class StarBurst2Shell : CostCardModelBase
{
    public StarBurst2Shell() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 3;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainStars(2m, base.Owner);
    }
}

/// <summary>王专属 #2:1 费,点数容量 10(普通)。自身效果:失去 1 星辉(星辉为 0 照打可出,LoseStars clamp 0)。</summary>
[Pool(typeof(PartSmithWangCostCardPool))]
public class StarDrain1Shell : CostCardModelBase
{
    public StarDrain1Shell() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.LoseStars(1m, base.Owner);
    }
}

/// <summary>王专属 #3:1 费,点数容量 6(罕见)。自身效果:获得 1 星辉。</summary>
[Pool(typeof(PartSmithWangCostCardPool))]
public class StarBurst1Shell : CostCardModelBase
{
    public StarBurst1Shell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 6;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainStars(1m, base.Owner);
    }
}

/// <summary>王专属 #4:2 费,点数容量 10(罕见)。自身效果:获得 3 星辉。</summary>
[Pool(typeof(PartSmithWangCostCardPool))]
public class StarBurst3Shell : CostCardModelBase
{
    public StarBurst3Shell() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainStars(3m, base.Owner);
    }
}

/// <summary>王专属 #5:2 费,点数容量 15(普通)。自身效果:失去 1 星辉。</summary>
[Pool(typeof(PartSmithWangCostCardPool))]
public class StarDrain1BigShell : CostCardModelBase
{
    public StarDrain1BigShell() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.LoseStars(1m, base.Owner);
    }
}

/// <summary>王专属 #6:3 费,点数容量 15(普通)。自身效果:获得 5 星辉。</summary>
[Pool(typeof(PartSmithWangCostCardPool))]
public class StarBurst5Shell : CostCardModelBase
{
    public StarBurst5Shell() : base(3, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainStars(5m, base.Owner);
    }
}

/// <summary>王专属 #7:1 费,点数容量 15(稀有)。自身效果:失去 2 星辉。</summary>
[Pool(typeof(PartSmithWangCostCardPool))]
public class StarDrain2Shell : CostCardModelBase
{
    public StarDrain2Shell() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.LoseStars(2m, base.Owner);
    }
}
