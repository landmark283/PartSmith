using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace PartSmith.PartSmithCode.Cards.Base;

/// <summary>
/// 拼接修饰器:把一个效果卡片拼到一张费用卡片上(实例级,挂在费用卡实例上)。
///
/// - 持久化:BaseLib 的 <see cref="CardModifier.ModifierSave"/> 自动存/读档,
///   这里通过 StoreSaveData/LoadSaveData 存效果卡 Id 与加入顺序(JoinIndex)。
/// - 描述:ModifyDescription 在拼卡描述末尾追加效果卡的效果文本(按列表序)。
/// - 执行:OnPlay 在拼卡打出时被 BaseLib 调用,按加入顺序依次执行效果脚本。
/// - 克隆进战斗:BaseLib 的 _modifiers.CopyOnClone 自动带上,顺序保留。
/// </summary>
public class EffectAttachmentModifier : CardModifier
{
    private string? _effectCardId;
    private int _joinIndex;

    public string? EffectCardId
    {
        get => _effectCardId;
        set => _effectCardId = value;
    }

    public int JoinIndex
    {
        get => _joinIndex;
        set => _joinIndex = value;
    }

    public override void StoreSaveData(ModifierSave save)
    {
        save.AdditionalProperties["EffectCardId"] = _effectCardId ?? "";
        save.IntProperties["JoinIndex"] = _joinIndex;
    }

    public override void LoadSaveData(ModifierSave save)
    {
        save.AdditionalProperties.TryGetValue("EffectCardId", out string? id);
        _effectCardId = string.IsNullOrEmpty(id) ? null : id;
        save.IntProperties.TryGetValue("JoinIndex", out _joinIndex);
        Priority = _joinIndex; // Priority 不进 ModifierSave,读档后按加入顺序恢复

        // 关键词随效果卡转移到宿主(拼卡打出后按原版消耗/先手等)。
        // 拼接时转移过一次;读档时宿主的关键词若未随存档持久化,这里补上(幂等)。
        var effect = ResolveEffectCard();
        if (effect != null && Owner != null)
        {
            foreach (var keyword in effect.Keywords)
            {
                Owner.AddKeyword(keyword);
            }
        }
    }

    public override void ModifyDescription(Creature? target, ref string description)
    {
        var effect = ResolveEffectCard();
        if (effect == null)
        {
            return;
        }
        // 用宿主拼卡上下文(力量/易伤/虚弱/升级)刷新效果卡 DynamicVars 的预览值,
        // 效果文本里的 {Damage:diff()} 等占位符才能显示"打向该目标时"的实际数值。
        if (Owner != null)
        {
            effect.RefreshPreviewForHost(Owner, target);
        }
        // 把宿主所在牌堆传进去,让描述里的 {InCombat:...|}(BodySlam 等)在战斗中的手牌显示战斗附加信息。
        PileType pile = Owner?.Pile?.Type ?? PileType.None;
        // 宿主升级态:已升级 → 显示升级分支;处于升级预览(卡组/战斗预览屏)→ 显示升级分支并标绿;
        // 否则 → 基础分支。效果卡是 canonical 单例、IsUpgraded 恒为 false,必须在这里按宿主判定。
        UpgradeDisplay display = Owner switch
        {
            { IsUpgraded: true } => UpgradeDisplay.Upgraded,
            { UpgradePreviewType: not CardUpgradePreviewType.None } => UpgradeDisplay.UpgradePreview,
            _ => UpgradeDisplay.Normal,
        };
        string effectDesc = effect.GetEffectDescription(pile, target, display);
        description = string.IsNullOrEmpty(description)
            ? effectDesc
            : description + "\n" + effectDesc;
        // 升级能量:原版升级费用 -1 的替代效果(见 EffectCardModelBase.UpgradeEnergyGain)。
        // 只在宿主拼卡已升级(或处于升级预览)时,在描述末尾追加 "Gain N Energy."
        //(措辞参考原版 Gain {Energy:energyIcons()}。)。拼卡未升级不显示,效果卡奖励界面
        //(无宿主)也不显示。
        if (effect.UpgradeEnergyGain != 0 && Owner is { } host
            && (host.IsUpgraded || host.UpgradePreviewType == CardUpgradePreviewType.Combat))
        {
            var energyLoc = new LocString("cards", "PARTSMITH_UPGRADE_ENERGY");
            energyLoc.AddObj("energy", effect.UpgradeEnergyGain);
            description += "\n" + energyLoc.GetFormattedText();
        }
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var effect = ResolveEffectCard();
        if (effect == null)
        {
            return;
        }
        // 升级能量:升级拼卡打出时先获得能量(替代原版升级费用 -1,在效果之前发放,
        // 让后续效果(如 Havoc 自动打出的牌)能用上这 1 点能量)。
        if (effect.UpgradeEnergyGain != 0 && cardPlay.Card.IsUpgraded)
        {
            await PlayerCmd.GainEnergy(effect.UpgradeEnergyGain, cardPlay.Player);
        }
        await effect.ExecuteEffect(choiceContext, cardPlay);
    }

    /// <summary>
    /// 战斗事件钩子 AfterDeath:把"宿主拼卡所在战斗有生物死亡"转发给已拼效果卡。
    /// 效果卡是 canonical 单例、不在战斗牌堆,原生钩子派发不到它;但本修饰器
    /// (挂载于战斗牌堆里的宿主卡上)已被 BaseLib 注册为战斗钩子订阅者,这里把事件
    /// 按宿主实例(<see cref="CardModifier.Owner"/>)转发给效果卡自己处理。
    /// 只有重写了 <see cref="EffectCardModelBase.OnHostAfterDeath"/> 的效果卡才会响应。
    /// </summary>
    public override Task AfterDeath(
        PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
    {
        var effect = ResolveEffectCard();
        if (effect != null && Owner != null)
        {
            return effect.OnHostAfterDeath(Owner, choiceContext, creature, wasRemovalPrevented);
        }
        return Task.CompletedTask;
    }

    /// <summary>
    /// 宿主拼卡升级 → 把效果卡"升级才有的关键词"(如原版 Misery 升级带 Retain)
    /// 补加到宿主上。BaseLib 的 <c>UpgradeModifiers</c> postfix 在 <c>UpgradeInternal</c>
    /// 时遍历卡的修饰器调 <c>OnUpgrade()</c>,这里按效果卡需求补关键词。
    /// </summary>
    public override void OnUpgrade()
    {
        var effect = ResolveEffectCard();
        if (effect?.UpgradeKeyword is { } keyword && Owner != null)
        {
            Owner.AddKeyword(keyword);
        }
    }

    /// <summary>降级对称回收升级时加的关键词(少用,保持幂等)。</summary>
    public override void OnDowngrade()
    {
        base.OnDowngrade();
        var effect = ResolveEffectCard();
        if (effect?.UpgradeKeyword is { } keyword && Owner != null)
        {
            Owner.RemoveKeyword(keyword);
        }
    }

    public EffectCardModelBase? ResolveEffectCard()
    {
        if (_effectCardId == null)
        {
            return null;
        }
        return ModelDb.GetById<EffectCardModelBase>(ModelId.Deserialize(_effectCardId));
    }
}
