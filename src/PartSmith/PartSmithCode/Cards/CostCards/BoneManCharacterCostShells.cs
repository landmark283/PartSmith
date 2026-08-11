using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 骨头人(亡灵契约师)专属 7 张费用牌,只进 PartSmithBoneManCostCardPool。
/// 奥提斯 = 骨头人的召唤物(Player.Osty / IsOstyAlive;OstyCmd.Summon / CreatureCmd.Kill 原生 API)。
/// 公共池 17 张共享壳的骨头人瘦子类见 BoneManSharedCostShells.cs。
/// </summary>

/// <summary>骨头人专属 #1:0 费,点数容量 3(普通)。自身关键字:虚无(回合结束消失)。</summary>
[Pool(typeof(PartSmithBoneManCostCardPool))]
public class EtherealShell : CostCardModelBase
{
    public EtherealShell() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    public override int PointCapacity => 3;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Ethereal };
}

/// <summary>骨头人专属 #2:1 费,点数容量 15(罕见)。自身效果:力量-1(永久)。</summary>
[Pool(typeof(PartSmithBoneManCostCardPool))]
public class BoneStrengthCostShell : CostCardModelBase
{
    public BoneStrengthCostShell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, -1m, base.Owner.Creature, this);
    }
}

/// <summary>骨头人专属 #3:3 费,点数容量 45(稀有)。自身效果:力量-3(永久)。</summary>
[Pool(typeof(PartSmithBoneManCostCardPool))]
public class BoneStrengthCostHeavyShell : CostCardModelBase
{
    public BoneStrengthCostHeavyShell() : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 45;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, -3m, base.Owner.Creature, this);
    }
}

/// <summary>骨头人专属 #4:1 费,点数容量 12(普通)。自身效果:奥提斯死去(击杀自己的奥提斯)。
/// 若奥提斯已死/不在场 → 空打(损失能量,无效果,不拿点)。</summary>
[Pool(typeof(PartSmithBoneManCostCardPool))]
public class OstyDeathShell : CostCardModelBase
{
    public OstyDeathShell() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCapacity => 12;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (base.Owner.IsOstyAlive)
        {
            await CreatureCmd.Kill(base.Owner.Osty!, true);
        }
    }
}

/// <summary>骨头人专属 #5:0 费,点数容量 10(稀有)。自身效果:力量-2(永久)。</summary>
[Pool(typeof(PartSmithBoneManCostCardPool))]
public class BoneDualStrengthShell : CostCardModelBase
{
    public BoneDualStrengthShell() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, -2m, base.Owner.Creature, this);
    }
}

/// <summary>骨头人专属 #6:2 费,点数容量 6(罕见)。自身效果:给所有手牌增加虚无(回合结束消失)。</summary>
[Pool(typeof(PartSmithBoneManCostCardPool))]
public class EtherealHandShell : CostCardModelBase
{
    public EtherealHandShell() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 6;

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (CardModel hand in PileType.Hand.GetPile(base.Owner).Cards.ToList())
        {
            hand.AddKeyword(CardKeyword.Ethereal);
        }
        return Task.CompletedTask;
    }
}

/// <summary>骨头人专属 #7:1 费,点数容量 6(罕见)。自身效果:召唤 3(奥提斯死亡则复活并设 3 生命上限并回满血,活着则生命上限+3)。</summary>
[Pool(typeof(PartSmithBoneManCostCardPool))]
public class OstySummonShell : CostCardModelBase
{
    public OstySummonShell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 6;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await OstyCmd.Summon(choiceContext, base.Owner, 3m, this);
    }
}
