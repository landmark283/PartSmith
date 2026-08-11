using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using PartSmith.PartSmithCode.Cards.CostCards;

namespace PartSmith.PartSmithCode.Powers;

/// <summary>
/// 本回合敏捷 -1(回合结束自动恢复 +1):同 <see cref="CrumblingShellStrengthDownPower"/>,
/// 复刻原版 TemporaryDexterityPower(IsPositive=false)。
/// </summary>
public class CrumblingShellDexterityDownPower : TemporaryDexterityPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<CrumblingShell>();

    protected override bool IsPositive => false;

    public string? CustomPackedIconPath => "BaseLib/images/powers/baselib-power_temp_down.png";

    public string? CustomBigIconPath => "BaseLib/images/powers/big/baselib-power_temp_down_big.png";
}
