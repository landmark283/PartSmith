using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using PartSmith.PartSmithCode.Cards.CostCards;

namespace PartSmith.PartSmithCode.Powers;

/// <summary>
/// 本回合敏捷 -1(回合结束自动恢复):直接照原版 TemporaryDexterityPower(IsPositive=false) 机制。
/// 供共享壳 TempDownShell / TempDownBigShell 及猎人同名壳共用;OriginModel 指共享#5(TempDownShell)。
/// </summary>
public class TempDexterityDownPower : TemporaryDexterityPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<TempDownShell>();

    protected override bool IsPositive => false;

    public string? CustomPackedIconPath => "BaseLib/images/powers/baselib-power_temp_down.png";

    public string? CustomBigIconPath => "BaseLib/images/powers/big/baselib-power_temp_down_big.png";
}
