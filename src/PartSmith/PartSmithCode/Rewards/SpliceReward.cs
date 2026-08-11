using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Nodes.Screens.Overlays;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Runs;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Cards.Splicing;

namespace PartSmith.PartSmithCode.Rewards;

/// <summary>
/// 效果卡奖励:从效果池出 3 张(3 选 1)。选中的效果卡**不直接入组**,
/// 而是让玩家从卡组里手动挑一张能拼上的费用卡,当场拼接(容量校验走 SpliceController)。
///
/// 交互流程:
/// 1. 效果卡选择屏(3 选 1,可关闭跳过整槽)。
/// 2. 选中后预检卡组:没有任何费用卡能拼上(点数不足)→ 移除这张效果卡,
///    弹"点数不足"提示,退回到效果卡选择屏重新选。
/// 3. 有可拼目标 → 弹卡组选择(只放行容量够的费用卡,可取消)。
///    玩家取消 → 退回到效果卡选择屏重新选(可改选别的效果)。
/// 4. 选到目标 → AttachEffect,奖励领取完成。
///
/// 继承 <see cref="CardReward"/> 只重写 OnSelect;Populate/CanSkip/序列化全走基类(用池构造,可序列化)。
/// RewardsSetIndex=6,排在费用卡奖励(5)之后。
///
/// 多人同步:OnSelect 由 RewardsSetSynchronizer 在**所有机器**上运行(确定性复算)。
/// 效果卡选择与目标卡选择都走 PlayerChoiceSynchronizer 对称 reserve(owner 弹 UI 并 SyncLocalChoice,
/// 远端 WaitForRemoteChoice),拼接 mutation 对同一(目标卡, 效果卡)全机复算 → 状态跨机一致。
/// 不能再写 "if (!LocalContext.IsMe(Player)) return false;" 提前返回——那会让远端少 reserve
/// 一次,choice 计数器永久漂移,每次校验都报"信息不同步"(2026-08-10 修复的根因)。
/// </summary>
public class SpliceReward : CardReward
{
    private static readonly LocString SelectTargetPrompt = new("card_selection", "PARTSMITH_SPLICE_TARGET_PROMPT");
    private static readonly LocString InsufficientPointsLoc = new("card_selection", "PARTSMITH_SPLICE_INSUFFICIENT_POINTS");

    public override int RewardsSetIndex => 6;

    public override LocString Description => new LocString("card_selection", "PARTSMITH_SPLICE_REWARD_DESC");

    public SpliceReward(CardCreationOptions options, int cardCount, Player player)
        : base(options, cardCount, player)
    {
    }

    public override void Populate()
    {
        // 效果卡由基类从池里生成(基类 _cards 是私有字段,用公有 Cards 属性读)。
        if (!IsPopulated)
        {
            base.Populate();
        }
    }

    protected override async Task<bool> OnSelect()
    {
        var cards = Cards.ToList();
        var synchronizer = RunManager.Instance.PlayerChoiceSynchronizer;

        // 跳过按钮 = CardRewardAlternative 之一,由 CardRewardAlternative.Generate(this)
        // 在 CanSkip=true(默认)时生成 "Skip" 选项(渲染成 RewardAlternatives 容器里的按钮)。
        // 必须传它而不是空列表,否则没有跳过按钮。点跳过 → OptionSelected 返回 >=cards.Count
        // 的替代项索引 → 走下面的 return false,整槽不领取退回奖励界面。
        // ⚠ 必须在**所有机器**上无条件调用(镜像原版 CardReward.OnSelect 顶部同款):Generate 里会跑
        // Hook.ModifyCardRewardAlternatives,只在 owner 机器调会破坏确定性。
        var rewardOptions = CardRewardAlternative.Generate(this);

        // 效果卡选择(3 选 1):全机器对称。
        // 结构镜像原版 CardReward.OnSelect:owner 先弹 UI,再 ReserveChoiceId(全机器同一 ID),
        // owner SyncLocalChoice / 远端 WaitForRemoteChoice → 两边计数器同步增长。
        // 这一步是本奖励里**第一个** choice,必须先 reserve 再 await——否则计数器漂移,
        // nextChoiceIds 每次校验都被哈希,拼接后每次进战斗/出篝火都报"信息不同步"。
        NCardRewardSelectionScreen? screen = null;
        if (LocalContext.IsMe(Player))
        {
            screen = NCardRewardSelectionScreen.ShowScreen(
                cards.Select(c => new CardCreationResult(c)).ToList(),
                rewardOptions);
        }

        int? selected = null;
        uint choiceId = synchronizer.ReserveChoiceId(Player);
        if (LocalContext.IsMe(Player))
        {
            if (screen != null)
            {
                try
                {
                    selected = await screen.OptionSelected();
                }
                catch (TaskCanceledException)
                {
                    selected = null; // 屏幕被提前关闭(如整组跳过)→ 整槽跳过
                }
                NOverlayStack.Instance?.Remove(screen);
            }
            // TestMode 下无 UI(screen==null),selected 保持 null(跳过);同样要同步,计数器不漂移。
            synchronizer.SyncLocalChoice(Player, choiceId, PlayerChoiceResult.FromIndex(selected));
        }
        else
        {
            selected = (await synchronizer.WaitForRemoteChoice(Player, choiceId)).AsIndexOrNull();
        }

        if (selected == null || selected.Value < 0 || selected.Value >= cards.Count)
        {
            return false; // 玩家关闭/跳过(含点 "Skip" 替代项)→ 退回奖励界面,整槽未领取
        }

        if (cards[selected.Value] is not EffectCardModelBase effectCard)
        {
            return false;
        }

        // 预检卡组:没有任何费用卡能拼上 → 点数不足。提示后直接 return false 退回奖励页面,
        // 让玩家重击效果奖励再选(不要内部循环——否则效果卡明明没被拿走,选项却像少了)。
        bool anySplittable = PileType.Deck.GetPile(Player).Cards
            .Any(c => c is CostCardModelBase cost && SpliceController.CanSplice(cost, effectCard));
        if (!anySplittable)
        {
            if (LocalContext.IsMe(Player))
            {
                await SpliceToast.Show(InsufficientPointsLoc.GetFormattedText());
            }
            return false;
        }

        // 手动挑目标费用卡;Cancelable=true 允许取消。取消也退回奖励页面,玩家可重击。
        // FromDeckGeneric 内部自带对称 reserve + SyncLocalChoice/WaitForRemoteChoice,
        // 远端拿到的 target 是卡组里的**真实实例**(NetDeckCard.ToCardModel → player.Deck.Cards[DeckIndex]),
        // 所以下面 AttachEffect 能在每台机器上对同一张卡复算。
        var target = (await CardSelectCmd.FromDeckGeneric(
                Player,
                new CardSelectorPrefs(SelectTargetPrompt, 1) { Cancelable = true },
                c => c is CostCardModelBase cost && SpliceController.CanSplice(cost, effectCard)))
            .FirstOrDefault();
        if (target == null)
        {
            return false; // 玩家取消选目标 → 退回奖励页面
        }

        // 拼接 mutation:所有机器对同一(目标卡, 效果卡)确定性复算 → 卡组状态跨机一致。
        SpliceController.AttachEffect(target, effectCard);
        if (LocalContext.IsMe(Player))
        {
            Log.Info($"PartSmith: spliced {effectCard.Id} onto {target.Id} (used {SpliceController.UsedPoints(target)}/{((CostCardModelBase)target).PointCapacity})");
        }
        return true;
    }
}
