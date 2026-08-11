using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using PartSmith.PartSmithCode.Cards.CostCards;

namespace PartSmith.PartSmithCode.Powers;

/// <summary>
/// 本回合力量 -1(回合结束自动恢复 +1):直接照原版 CrushUnderPower / DarkShacklesPower 的写法。
/// 原版 TemporaryStrengthPower(IsPositive=false) 即为"应用时立即扣、回合结束移除并补回"。
/// 实现 ICustomPower 把图标指到 BaseLib 自带的临时减益图标(CustomTemporaryPowerModelWrapper
/// 用的同一路径),避免自定义 power 缺图。
/// </summary>
public class CrumblingShellStrengthDownPower : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<CrumblingShell>();

    protected override bool IsPositive => false;

    public string? CustomPackedIconPath => "BaseLib/images/powers/baselib-power_temp_down.png";

    public string? CustomBigIconPath => "BaseLib/images/powers/big/baselib-power_temp_down_big.png";
}
