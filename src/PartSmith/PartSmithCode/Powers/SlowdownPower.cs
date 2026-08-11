using System;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace PartSmith.PartSmithCode.Powers;

/// <summary>
/// 动作缓慢(Slowdown):每次抽牌按抽牌数消耗层数,少抽等量的牌。
/// - 层数 &lt; 抽牌数:实际抽数 = 抽牌数 - 层数,层数清 0。
/// - 层数 ≥ 抽牌数:不抽牌,层数 -= 抽牌数。
/// 即:每抽 1 张牌要花 1 层动作缓慢,不够扣就不抽。层数抽完即消失(战斗中持续,不随回合重置)。
/// - 回合开始抽牌:CombatManager 走 Hook.ModifyHandDraw → 本 Power 的 ModifyHandDraw 消耗+减数。
/// - 卡片效果抽牌(CardPileCmd.Draw,fromHandDraw=false):由 DrawSlowdownPrefixPatch 的
///   Harmony prefix 消耗+减数(fromHandDraw=true 已由 ModifyHandDraw 消耗 → prefix 跳过,防双扣)。
/// 图标复用 BaseLib 临时减益图标(与 TempStrengthDownPower / TempDexterityDownPower 同一路径)。
/// </summary>
public class SlowdownPower : PowerModel, ICustomPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (player != base.Owner.Player)
        {
            return count;
        }
        return Consume(count);
    }

    public override Task AfterModifyingHandDraw()
    {
        Flash();
        return Task.CompletedTask;
    }

    /// <summary>
    /// 按一次抽牌数消耗动作缓慢层数并返回实际抽牌数(与 DrawSlowdownPrefixPatch 共用同一公式)。
    /// 消耗到 0 时自我移除(抽完即消失)。
    /// </summary>
    public decimal Consume(decimal count)
    {
        int slow = base.Amount;
        if (slow <= 0)
        {
            return count;
        }
        int requested = count > 0m ? (int)Math.Ceiling(count) : 0;
        int consumed = Math.Min(slow, requested);
        if (consumed <= 0)
        {
            return count;
        }
        if (slow > consumed)
        {
            SetAmount(slow - consumed);
        }
        else
        {
            RemoveInternal();
        }
        Flash();
        return count - (decimal)consumed;
    }

    public string? CustomPackedIconPath => "BaseLib/images/powers/baselib-power_temp_down.png";

    public string? CustomBigIconPath => "BaseLib/images/powers/big/baselib-power_temp_down_big.png";
}
