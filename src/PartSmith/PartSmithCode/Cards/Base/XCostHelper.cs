#nullable disable
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace PartSmith.PartSmithCode.Cards.Base;

/// <summary>
/// X 费拼卡辅助。
///
/// 效果卡不是原版 <c>HasEnergyCostX</c> 卡,不能调 <c>ResolveEnergyXValue()</c>(它检查 CostsX,
/// 效果卡单例恒为 false → 直接 throw "This card does not have an X-cost.")。
///
/// 拼卡语义 = 原版 X 费:打出时 X = 当前能量(宿主壳费用已在打出流程扣掉,故 X 是不含壳费用的余额),
/// 随后立刻把玩家能量花光(对应原版 X 费"全花剩余能量")。升级等其余逻辑由各 Fragment 自行处理
/// (X+1 或伤害/召唤数值 +1)。
/// </summary>
public static class XCostHelper
{
    /// <summary>返回 X(打出时的当前能量)并把所有能量花光。</summary>
    public static async Task<int> ResolveAndSpend(CardPlay cardPlay)
    {
        int x = cardPlay.Player.PlayerCombatState.Energy;
        await PlayerCmd.LoseEnergy(x, cardPlay.Player);
        return x;
    }
}
