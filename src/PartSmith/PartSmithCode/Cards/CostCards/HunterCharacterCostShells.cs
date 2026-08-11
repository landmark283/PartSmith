using System.Linq;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using PartSmith.PartSmithCode.Powers;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 小猎人专属 7 张费用牌,只进 PartSmithHunterCostCardPool。
/// 公共池 17 张共享壳的猎人瘦子类见 HunterSharedCostShells.cs。
/// </summary>

/// <summary>猎人专属 #1:0 费,点数容量 6(罕见)。自身效果:本回合力量-1、敏捷-1。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class QuickCurseShell : CostCardModelBase
{
    public QuickCurseShell() : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 6;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<TempStrengthDownPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
        await PowerCmd.Apply<TempDexterityDownPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }
}

/// <summary>猎人专属 #2:1 费,点数容量 15(稀有)。自身效果:本回合力量-1、敏捷-1。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class GreatCurseShell : CostCardModelBase
{
    public GreatCurseShell() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<TempStrengthDownPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
        await PowerCmd.Apply<TempDexterityDownPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }
}

/// <summary>猎人专属 #3:1 费,点数容量 10(稀有)。自身效果:给选定的敌人施加 2 层人工制品。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class SaboteurShell : CostCardModelBase
{
    public SaboteurShell() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }
        await PowerCmd.Apply<ArtifactPower>(choiceContext, cardPlay.Target, 2m, base.Owner.Creature, this);
    }
}

/// <summary>猎人专属 #4:0 费,点数容量 15(稀有)。自身效果:获得 3 层动作缓慢。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class SlowdownBurstShell : CostCardModelBase
{
    public SlowdownBurstShell() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SlowdownPower>(choiceContext, base.Owner.Creature, 3m, base.Owner.Creature, this);
    }
}

/// <summary>猎人专属 #5:2 费,点数容量 6(罕见)。自身效果:抽 3 张牌。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class DrawSpreeShell : CostCardModelBase
{
    public DrawSpreeShell() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 6;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 3m, base.Owner);
    }
}

/// <summary>猎人专属 #6:1 费,点数容量 10(罕见)。自身效果:丢弃 3 张手牌。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class DiscardSpreeShell : CostCardModelBase
{
    public DiscardSpreeShell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cards = await CardSelectCmd.FromHand(
            choiceContext, base.Owner, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 3, 3), null, this);
        await CardCmd.Discard(choiceContext, cards);
    }
}

/// <summary>猎人专属 #7:3 费,点数容量 15(普通)。自身效果:先抽 3 张牌,再丢弃 2 张手牌。</summary>
[Pool(typeof(PartSmithHunterCostCardPool))]
public class FilterShell : CostCardModelBase
{
    public FilterShell() : base(3, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 3m, base.Owner);
        var cards = await CardSelectCmd.FromHand(
            choiceContext, base.Owner, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 2, 2), null, this);
        await CardCmd.Discard(choiceContext, cards);
    }
}
