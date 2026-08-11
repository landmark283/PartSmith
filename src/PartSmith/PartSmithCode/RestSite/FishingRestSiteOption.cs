using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Cards.Splicing;
using PartSmith.PartSmithCode.Rewards;

namespace PartSmith.PartSmithCode.RestSite;

/// <summary>
/// 篝火"钓鱼":把完整拼卡拆成"费用牌(留卡组)+ 效果牌(进暂存奖励池)",
/// 再把池里的效果牌自由拼回(顺序 = 拼接顺序,实现重新组合/重排)。
///
/// 交互流程:
/// 1. 拆解(Phase A):循环弹出"卡组中可拆拼卡"选择,每选一张就把它的全部效果卸进池里;
///    ESC 进入重组。没有可拆的拼卡也直接进入重组。
/// 2. 重组(Phase B):循环"从池里选一张效果卡 → 从卡组选一张能拼上的费用卡"拼接;
///    选目标时 ESC 等于放回继续挑,在选效果卡时 ESC 才结束。
/// 3. 什么都没做 → OnSelect 返回 false(选项不消耗,篝火可继续选治疗/锻造);
///    做了任何动作 → 返回 true(消耗本次篝火);结束时池里剩余效果卡丢弃(放生)。
///
/// 多人同步:OnSelect 由 RestSiteSynchronizer 在**所有机器**上运行。FromDeckGeneric/FromSimpleGrid
/// 内部自带对称 reserve,拆解/拼接 mutation 对同步过的同一张卡全机复算 → 状态跨机一致。
/// 提示 toast 是纯 UI,只在 owner 机器显示(其余机器不弹)。
/// </summary>
public class FishingRestSiteOption : CustomRestSiteOption
{
    private static readonly LocString DisassemblePrompt = new("card_selection", "PARTSMITH_FISH_DISASSEMBLE_PROMPT");
    private static readonly LocString SpliceEffectPrompt = new("card_selection", "PARTSMITH_FISH_SPLICE_EFFECT_PROMPT");
    private static readonly LocString SpliceTargetPrompt = new("card_selection", "PARTSMITH_FISH_SPLICE_TARGET_PROMPT");
    private static readonly LocString NothingToDisassemble = new("card_selection", "PARTSMITH_FISH_NO_SPLICED_CARDS");

    public override string OptionId => "PARTSMITH_FISH";

    /// <summary>
    /// 自定义图标(整条 res:// 路径)。复用原版"挖掘"(option_dig)图标——钓鱼=拆解+重组,主题最贴近;
    /// 也避免默认路径 ui/rest_site/option_partsmith_fish.png 在 pck 里不存在(按钮无图标)。
    /// </summary>
    public override string? CustomIconPath => ImageHelper.GetImagePath("ui/rest_site/option_dig.png");

    public FishingRestSiteOption(Player owner) : base(owner) { }

    public override async Task<bool> OnSelect()
    {
        var staged = new List<CardModel>();
        bool anythingChanged = false;

        // Phase A —— 拆解(可连续拆多张;ESC 进入重组)。
        while (true)
        {
            if (!PileType.Deck.GetPile(Owner).Cards.Any(SpliceController.IsSpliced))
            {
                if (!anythingChanged && LocalContext.IsMe(Owner))
                {
                    await SpliceToast.Show(NothingToDisassemble.GetFormattedText());
                }
                break;
            }

            var pick = (await CardSelectCmd.FromDeckGeneric(
                Owner,
                new CardSelectorPrefs(DisassemblePrompt, 1) { Cancelable = true },
                SpliceController.IsSpliced)).ToList();
            if (pick.Count == 0)
            {
                break; // ESC → 进入重组
            }

            var effects = SpliceController.DetachAllEffects(pick[0]);
            staged.AddRange(effects);
            anythingChanged = true;

            if (LocalContext.IsMe(Owner))
            {
                var toast = new LocString("card_selection", "PARTSMITH_FISH_CAUGHT");
                toast.AddObj("count", effects.Count);
                await SpliceToast.Show(toast.GetFormattedText());
            }
        }

        // Phase B —— 重组(把池里效果卡自由拼回;顺序 = 拼接顺序;选效果卡时 ESC 结束)。
        while (staged.Count > 0)
        {
            var effPick = (await CardSelectCmd.FromSimpleGrid(
                new BlockingPlayerChoiceContext(),
                staged,
                Owner,
                new CardSelectorPrefs(SpliceEffectPrompt, 1))).ToList();
            if (effPick.Count == 0)
            {
                break; // ESC → 结束,池里剩余效果卡丢弃
            }

            var effect = (EffectCardModelBase)effPick[0];
            var target = (await CardSelectCmd.FromDeckGeneric(
                Owner,
                new CardSelectorPrefs(SpliceTargetPrompt, 1) { Cancelable = true },
                c => c is CostCardModelBase cost && SpliceController.CanSplice(cost, effect))).ToList();
            if (target.Count == 0)
            {
                continue; // ESC 取消选目标 → 放回池里,继续挑别的
            }

            SpliceController.AttachEffect(target[0], effect);
            staged.Remove(effect);
            anythingChanged = true;
            if (LocalContext.IsMe(Owner))
            {
                Log.Info($"PartSmith fishing: spliced {effect.Id} back onto {target[0].Id} " +
                         $"(used {SpliceController.UsedPoints(target[0])}/{((CostCardModelBase)target[0]).PointCapacity})");
            }
        }

        return anythingChanged;
    }
}
