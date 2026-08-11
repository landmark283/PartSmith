using System.Collections.Generic;
using System.Threading.Tasks;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Pools;
using PartSmith.PartSmithCode.Powers;

namespace PartSmith.PartSmithCode.Cards.CostCards;

/// <summary>
/// 设计"公共池"17 张共享费用牌(所有角色都有)。战士池写基类壳,
/// 猎人/王/骨头人各写瘦子类只改 [Pool](见 HunterSharedCostShells / WangSharedCostShells / BoneManSharedCostShells)。
/// 改效果/点数/稀有度只改这里基类一处,各角色自动同步。
///
/// 实现约定:名字随便起(拼卡显示名=效果名,壳名仅空壳时出现)。
/// 公共池(编号=费用kards.md 公共池 #1-#17):
///   #1 0费3点普通 / #2 0费6点稀有 / #3 1费6点普通 / #4 1费10点稀有 / #5 1费10点本回-1str-1dex 普通
///   #6 1费10点消耗 罕见 / #7 2费10点普通 / #8 2费15点本回-1str-1dex 普通 / #9 2费15点稀有
///   #10 3费18点普通 / #11 3费30点虚无消耗 稀有 / #12 3费30点力量-1 稀有
///   #13 1费15点自己1层缓慢 罕见 / #14 2费20点自己1层缓慢 罕见
///   #15 1费10点丢弃1张牌 稀有 / #16 1费10点弃牌堆加眩晕 罕见 / #17 1费10点敌人获得2点力量 罕见
/// </summary>

/// <summary>共享 #1:0 费,点数容量 3(普通),无自身效果。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class TrinketShell : CostCardModelBase
{
    public TrinketShell() : base(0, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    public override int PointCapacity => 3;
}

/// <summary>共享 #2:0 费,点数容量 6(稀有),无自身效果。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class GemShell : CostCardModelBase
{
    public GemShell() : base(0, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    public override int PointCapacity => 6;
}

/// <summary>共享 #3:1 费,点数容量 6(普通),无自身效果。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class BoardShell : CostCardModelBase
{
    public BoardShell() : base(1, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    public override int PointCapacity => 6;
}

/// <summary>共享 #4:1 费,点数容量 10(稀有),无自身效果。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class IngotShell : CostCardModelBase
{
    public IngotShell() : base(1, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    public override int PointCapacity => 10;
}

/// <summary>共享 #5:1 费,点数容量 10(普通)。自身效果:本回合力量-1、敏捷-1(回合结束自动恢复)。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class TempDownShell : CostCardModelBase
{
    public TempDownShell() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<TempStrengthDownPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
        await PowerCmd.Apply<TempDexterityDownPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }
}

/// <summary>共享 #6:1 费,点数容量 10(罕见)。自身关键字:消耗(打出即进消耗堆)。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class ExhaustShell : CostCardModelBase
{
    public ExhaustShell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.None)
    {
    }

    public override int PointCapacity => 10;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };
}

/// <summary>共享 #7:2 费,点数容量 10(普通),无自身效果。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class FortShell : CostCardModelBase
{
    public FortShell() : base(2, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    public override int PointCapacity => 10;
}

/// <summary>共享 #8:2 费,点数容量 15(普通)。自身效果:本回合力量-1、敏捷-1(回合结束自动恢复)。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class TempDownBigShell : CostCardModelBase
{
    public TempDownBigShell() : base(2, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<TempStrengthDownPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
        await PowerCmd.Apply<TempDexterityDownPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }
}

/// <summary>共享 #9:2 费,点数容量 15(稀有),无自身效果。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class KeepShell : CostCardModelBase
{
    public KeepShell() : base(2, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    public override int PointCapacity => 15;
}

/// <summary>共享 #10:3 费,点数容量 18(普通),无自身效果。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class CraneShell : CostCardModelBase
{
    public CraneShell() : base(3, CardType.Skill, CardRarity.Common, TargetType.None)
    {
    }

    public override int PointCapacity => 18;
}

/// <summary>共享 #11:3 费,点数容量 30(稀有)。自身关键字:虚无、消耗。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class VoidExhaustShell : CostCardModelBase
{
    public VoidExhaustShell() : base(3, CardType.Skill, CardRarity.Rare, TargetType.None)
    {
    }

    public override int PointCapacity => 30;

    public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust, CardKeyword.Ethereal };
}

/// <summary>共享 #12:3 费,点数容量 30(稀有)。自身效果:力量-1(永久)。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class WeakenShell : CostCardModelBase
{
    public WeakenShell() : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 30;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, base.Owner.Creature, -1m, base.Owner.Creature, this);
    }
}

/// <summary>共享 #13:1 费,点数容量 15(罕见)。自身效果:获得 1 层动作缓慢(永久到战斗结束)。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class SlowdownShell : CostCardModelBase
{
    public SlowdownShell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 15;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SlowdownPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }
}

/// <summary>共享 #14:2 费,点数容量 20(罕见)。自身效果:获得 1 层动作缓慢(永久到战斗结束)。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class SlowdownBigShell : CostCardModelBase
{
    public SlowdownBigShell() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 20;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SlowdownPower>(choiceContext, base.Owner.Creature, 1m, base.Owner.Creature, this);
    }
}

/// <summary>共享 #15:1 费,点数容量 10(稀有)。自身效果:丢弃 1 张手牌。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class DiscardShell : CostCardModelBase
{
    public DiscardShell() : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var cards = await CardSelectCmd.FromHand(
            choiceContext, base.Owner, new CardSelectorPrefs(CardSelectorPrefs.DiscardSelectionPrompt, 1, 1), null, this);
        await CardCmd.Discard(choiceContext, cards);
    }
}

/// <summary>共享 #16:1 费,点数容量 10(罕见)。自身效果:弃牌堆中增加一张眩晕。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class DazeShell : CostCardModelBase
{
    public DazeShell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel card = base.CombatState!.CreateCard<Dazed>(base.Owner);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Discard, base.Owner));
    }
}

/// <summary>共享 #17:1 费,点数容量 10(罕见)。自身效果:给指定的敌人增加 2 点力量(磨砺敌人)。</summary>
[Pool(typeof(PartSmithCostCardPool))]
public class EmpowerShell : CostCardModelBase
{
    public EmpowerShell() : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override int PointCapacity => 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Target == null)
        {
            return;
        }
        await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, 2m, base.Owner.Creature, this);
    }
}
