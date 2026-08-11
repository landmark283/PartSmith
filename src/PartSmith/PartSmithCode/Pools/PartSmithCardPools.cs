using BaseLib.Abstracts;
using Godot;

namespace PartSmith.PartSmithCode.Pools;

/// <summary>
/// 费用卡片专属池:费用卡不进原版角色池,只通过本 mod 的奖励/命令出现。
/// </summary>
public class PartSmithCostCardPool : CustomCardPoolModel
{
    public override string Title => "partsmith_cost";

    // 必须 IsShared=true 才会被注册进 ModelDb.AllSharedCardPools(BaseLib patch),
    // 否则不在 AllCardPools 里,card.Pool 会抛 "not in any card pool" 异常。
    public override bool IsShared => true;

    public override Color DeckEntryCardColor => new Color("8E7CC3");

    /// <summary>默认已发现:百科大全里直接显示完整卡面(否则是"未见"剪影)。</summary>
    public override bool SeenByDefault => true;

    public override bool IsColorless => true;

    /// <summary>
    /// 卡面费用图标 = 原版铁甲战士图标(与原作一致)。
    /// 机制:CardModel.EnergyIcon → VisualCardPool.EnergyIconPath → EnergyIconHelper.GetPath(EnergyColorName);
    /// BaseLib 的 CustomEnergyIconPatches.IconPatch 拦截 GetPath,卡池实现 ICustomEnergyIconPool 且
    /// BigEnergyIconPath 非空时直接返回该路径;为 null 则回退 ui_atlas 里不存在的
    /// "energy_{card_pool∴partsmith_...}.tres" → 图标加载失败(日志:Missing sprite 'card/energy_card_pool∴...' in ui_atlas)。
    /// </summary>
    public override string? BigEnergyIconPath => "res://images/atlases/ui_atlas.sprites/card/energy_ironclad.tres";

    /// <summary>卡牌描述里内嵌的能量图标(如 {EnergyCostIcon})同用原版铁甲战士文本图标。</summary>
    public override string? TextEnergyIconPath => "res://images/packed/sprite_fonts/ironclad_energy_icon.png";

    /// <summary>费用文字描边色 = 原版 ironclad,与上面的铁甲战士图标融合(CardPoolModel 注释要求必须与图标配色一致)。</summary>
    public override Color EnergyOutlineColor => new Color("802020");
}

/// <summary>
/// 效果卡片专属池:效果卡不进原版角色池,只通过本 mod 的奖励/命令出现。
/// </summary>
public class PartSmithEffectCardPool : CustomCardPoolModel
{
    public override string Title => "partsmith_effect";

    public override bool IsShared => true;

    public override Color DeckEntryCardColor => new Color("7CC38E");

    /// <summary>默认已发现:百科大全里直接显示完整卡面(否则是"未见"剪影)。</summary>
    public override bool SeenByDefault => true;

    public override bool IsColorless => true;

    /// <summary>卡面费用图标 = 原版铁甲战士图标(与原作一致),同上 <see cref="PartSmithCostCardPool"/>。</summary>
    public override string? BigEnergyIconPath => "res://images/atlases/ui_atlas.sprites/card/energy_ironclad.tres";

    /// <summary>卡牌描述里内嵌的能量图标同用原版铁甲战士文本图标。</summary>
    public override string? TextEnergyIconPath => "res://images/packed/sprite_fonts/ironclad_energy_icon.png";

    /// <summary>费用文字描边色 = 原版 ironclad,与图标融合。</summary>
    public override Color EnergyOutlineColor => new Color("802020");
}

/// <summary>
/// 小猎人费用卡片专属池:与 <see cref="PartSmithCostCardPool"/> 同构,但费用图标换成原版猎人(silent 绿)。
/// 小猎人的费用卡 = 14 个 Hunter 壳子类(复用战士壳全部逻辑,见 Cards/CostCards/HunterCostShells.cs),
/// 卡池归属本池 → 卡面费用图标/描边自动走猎人配色,不动大战士的 ironclad 池。
/// 图标资源(游戏 pck 已核实存在):
/// BigEnergyIconPath = energy_silent.tres,TextEnergyIconPath = silent_energy_icon.png,描边色 = #1A6625(原版 Silent 池配色)。
/// </summary>
public class PartSmithHunterCostCardPool : CustomCardPoolModel
{
    public override string Title => "partsmith_hunter_cost";

    public override bool IsShared => true;

    public override Color DeckEntryCardColor => new Color("1A6625");

    public override bool SeenByDefault => true;

    public override bool IsColorless => true;

    public override string? BigEnergyIconPath => "res://images/atlases/ui_atlas.sprites/card/energy_silent.tres";

    public override string? TextEnergyIconPath => "res://images/packed/sprite_fonts/silent_energy_icon.png";

    public override Color EnergyOutlineColor => new Color("1A6625");
}

/// <summary>
/// 小猎人效果卡片专属池:与 <see cref="PartSmithEffectCardPool"/> 同构,费用图标走猎人 silent 绿。
/// 阶段 B2 生成的猎人效果卡(<原版猎人卡>Fragment)注册进本池。
/// </summary>
public class PartSmithHunterEffectCardPool : CustomCardPoolModel
{
    public override string Title => "partsmith_hunter_effect";

    public override bool IsShared => true;

    public override Color DeckEntryCardColor => new Color("4C9E5A");

    public override bool SeenByDefault => true;

    public override bool IsColorless => true;

    public override string? BigEnergyIconPath => "res://images/atlases/ui_atlas.sprites/card/energy_silent.tres";

    public override string? TextEnergyIconPath => "res://images/packed/sprite_fonts/silent_energy_icon.png";

    public override Color EnergyOutlineColor => new Color("1A6625");
}
