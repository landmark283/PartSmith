using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;

namespace PartSmith.PartSmithCode.Cards.Base;

/// <summary>
/// 效果卡片基类:带一个"点数消耗 p" + 一个效果脚本。
/// 效果卡本身不会被单独打出、也不会单独进卡组;它被拼接到费用卡上,
/// 由 <see cref="EffectAttachmentModifier"/> 在拼卡打出时调用 <see cref="ExecuteEffect"/>。
/// 拼接时用 <see cref="PointCost"/> 做容量校验。
/// </summary>
public abstract class EffectCardModelBase(int cost, CardType type, CardRarity rarity, TargetType target) :
    PartSmithCard(cost, type, rarity, target, showInCardLibrary: true)
{
    /// <summary>点数消耗 p。</summary>
    public virtual int PointCost => 0;

    /// <summary>
    /// 星费:原版储君卡"能量 + 辉星"双轨费里的辉星部分(如 FallingStar 打 8 需 2 星)。
    /// 效果卡本身不被单独打出、星费不直接生效;拼接时由
    /// <see cref="SpliceController.RefreshHostStarCost"/> 累加写到宿主壳的 <c>_baseStarCost</c>(方案 A 宿主携带星费),
    /// 基游戏的打出流程随后免费接管:能量/星不足 → 灰显不可点;打出 → 自动 SpendStars;
    /// <c>LastStarsSpent</c> 就位 → <c>ResolveStarXValue()</c> 可用。0 = 无星费(默认)。
    /// </summary>
    public virtual int StarCost => 0;

    /// <summary>
    /// 消耗后逐回合自动复播(原版 HowlFromBeyond 行为)。效果卡是 canonical 单例、不在战斗牌堆,
    /// AfterAutoPostPlayPhaseEntered 的 combat hook 只会派发到宿主拼卡,所以复播判定放在
    /// <see cref="CostCardModelBase"/> 层:宿主在消耗牌堆且拼了本效果时 AutoPlay。
    /// 注意需配合 <see cref="CanonicalKeywords"/> 里的 Exhaust,拼接时才会把消耗转移到宿主。
    /// </summary>
    public virtual bool ReplayWhenExhausted => false;

    /// <summary>
    /// 升级能量:原版这 11 张卡的升级是能量费用 -1(<c>EnergyCost.UpgradeBy(-1)</c>)。
    /// 效果卡恒 0 费、从不单独打出,费用 -1 对效果卡自身无意义,用户定为替代效果:
    /// 升级后的拼卡打出时,本回合获得 <see cref="UpgradeEnergyGain"/> 点能量。
    /// 发放与显示统一在 <see cref="EffectAttachmentModifier"/>:OnPlay 先 GainEnergy,
    /// ModifyDescription 在宿主已升级/升级预览时追加 "Gain N Energy."(本地化键 PARTSMITH_UPGRADE_ENERGY)。
    /// </summary>
    public virtual int UpgradeEnergyGain => 0;

    /// <summary>
    /// 宿主拼卡升级时额外获得的关键词(原版卡升级带 Retain 等关键词时的替代)。
    /// 效果卡自身的关键词在拼接时转移(见 <see cref="SpliceController.AttachEffect"/>);
    /// 升级才有的关键词没有拼接时机可转移,由 <see cref="EffectAttachmentModifier.OnUpgrade"/>
    /// 在宿主升级时补加到宿主上(反之 <c>OnDowngrade</c> 移除)。默认无。
    /// </summary>
    public virtual CardKeyword? UpgradeKeyword => null;

    /// <summary>
    /// 本效果卡能吃到的宿主升级级数上限。默认 1:壳升再多级也只吃到一级增量
    /// (惰性加载仍按宿主升级态叠加,只是封顶 1 级)。
    /// 少数"可多次升级"卡(如灼热打击式)override 成更大值(或 int.MaxValue),
    /// 此时增量按 <see cref="EffectiveUpgradeLevels"/> 级数叠加(每级一份 upgrade.vars 增量)。
    /// 由生成脚本按 partsmith_effect_cards.json 的 upgrade.levels 内联成常量。
    /// </summary>
    public virtual int MaxUpgradeLevels => 1;

    /// <summary>效果脚本:拼卡打出时执行。宿主卡 = cardPlay.Card(攻击来源等用它)。</summary>
    public abstract Task ExecuteEffect(PlayerChoiceContext choiceContext, CardPlay cardPlay);

    /// <summary>
    /// 拼接完成时回调(战斗外,奖励/篝火/parttest)。效果卡可为宿主预创建所需的状态修饰器
    /// (如 TheScythe 的成长暂存器 <see cref="Splicing.ScytheExtraModifier"/>)。
    ///
    /// 为什么必须在拼接时预创建而不是 OnPlay 里懒创建:
    /// BaseLib 的 AfterCardPlayedPatch 用 foreach 遍历卡的 modifiers 调 OnPlay,
    /// OnPlay 里再 <c>CardModifier.AddModifier</c> 会"修改正在遍历的集合"抛异常。
    /// </summary>
    public virtual void OnSplicedToHost(CardModel host)
    {
    }

    /// <summary>
    /// 宿主拼卡经历事件钩子时回调效果卡(事件语义由宿主/修饰器按需转发,默认无响应)。
    /// 效果卡是 canonical 单例、不在战斗牌堆,原生事件钩子派发不到它身上;但它附着的
    /// <see cref="EffectAttachmentModifier"/> 已被 BaseLib 注册为战斗钩子订阅者,
    /// 由修饰器把 <c>AfterDeath</c> 等事件按宿主实例转发到这里。重写此方法可实现
    /// "响应其他生物死亡"这类非 OnPlay 效果(如 Melancholy 的死亡加费)。
    /// <paramref name="host"/> 是触发事件的宿主拼卡实例(per-host 上下文,可取 EnergyCost 等)。
    /// </summary>
    public virtual Task OnHostAfterDeath(
        CardModel host, PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented)
        => Task.CompletedTask;

    /// <summary>
    /// 卡图来源:效果卡 1:1 镜像的原版卡。生成脚本为每张卡 override 成对应原版类型,
    /// 效果卡不自己做图,直接复用原版原画(单张显示走原版 atlas,拼图走原版独立 PNG)。
    /// 5 张 demo 卡(Strike/Block/Guard/Power/DrawFragment)手动指定对应的原版卡。
    /// </summary>
    protected virtual CardModel? PortraitSourceCard => null;

    /// <summary>卡面 = 原版卡卡面;无来源时回落 PartSmithCard 默认路径(通常不存在→占位)。</summary>
    public override string PortraitPath => PortraitSourceCard?.PortraitPath ?? base.PortraitPath;

    /// <summary>
    /// 卡面纹理 = 原版卡卡面(奖励选择屏/预览显示用)。
    /// 效果卡是 CustomCardModel,BaseLib 的 CustomCardPortrait patch 会拦截非 virtual 的
    /// CardModel.Portrait:CustomPortrait 为 null 时它走 CustomPortraitPath(mod 里不存在的
    /// PNG)→ ResourceLoader.Load 返回 null → 界面无图。这里直接给原版卡纹理
    /// (PortraitSourceCard 不是 CustomCardModel,patch 放行原始加载,返回原版 atlas 卡面)。
    /// 与拼图无关:CardArtSplicer 的切片源是独立 PNG(SplicePngPath),不走本属性。
    /// </summary>
    public override Texture2D? CustomPortrait => PortraitSourceCard?.Portrait;

    /// <summary>Beta 卡面:直接复用正常卡面(原版卡几乎不重写 BetaPortraitPath,beta .tres 不存在会花图)。</summary>
    public override string BetaPortraitPath => PortraitSourceCard?.PortraitPath ?? base.BetaPortraitPath;

    /// <summary>
    /// 原版卡独立原画 PNG。覆写 CardModel 的 protected virtual PortraitPngPath:
    /// 效果卡自身 Id 的 PNG 不存在,改由原版镜像卡解析,HasPortrait 等也能正确识别。
    /// </summary>
    protected override string PortraitPngPath => PortraitPngOf(PortraitSourceCard);

    /// <summary>拼图切片源 = 原版卡独立原画 PNG(纯图,Texture2D.GetImage() 可安全切片)。</summary>
    public string SplicePngPath => PortraitPngPath;

    /// <summary>原版卡 beta 原画 PNG(个别卡如 Blaze/Outrage 无普通原画、只有 beta 原画,拼图时回退)。</summary>
    public string SpliceBetaPngPath => PortraitPngOf(PortraitSourceCard, beta: true);

    /// <summary>
    /// 原版卡卡面纹理(拼图缺原画时的回退用)。直接取原版卡的 Portrait:
    /// 原版卡不是 CustomCardModel,BaseLib 的 CustomCardPortrait patch 会放行原始逻辑,
    /// 加载到原版 atlas 卡面——不走本效果卡的 CustomPortraitPath(指向 mod 里不存在的 PNG)。
    /// </summary>
    public Texture2D? SourcePortraitTexture => PortraitSourceCard?.Portrait;

    private static string PortraitPngOf(CardModel? src, bool beta = false)
        => src == null ? ""
        : ImageHelper.GetImagePath($"packed/card_portraits/{src.Pool.Title.ToLowerInvariant()}/{(beta ? "beta/" : "")}{src.Id.Entry.ToLowerInvariant()}.png");

    /// <summary>拼接后显示在拼卡描述里的效果文本(效果卡自己的描述)。</summary>
    public string GetEffectDescription(Creature? target)
        => GetEffectDescription(PileType.None, target);

    /// <summary>
    /// 同 <see cref="GetEffectDescription(Creature?)"/>,但可指定描述所在的牌堆类型。
    /// 游戏描述里的 <c>{InCombat:...|}</c> 占位符用"所在牌堆是否为战斗牌堆"判定是否显示战斗附加信息
    /// (CardModel.GetDescriptionForPile 里 <c>InCombat = CombatManager.IsInProgress &amp;&amp; (Pile?.IsCombatPile ?? pileType.IsCombatPile())</c>)。
    /// 效果卡是 canonical 单例、Pile 恒为 null,所以必须把宿主拼卡所在牌堆传进来,
    /// 战斗中的手牌(Hand)才能让 <c>{InCombat}</c> 分支显示。非战斗场景传 None/其余牌堆,只显示基础文本。
    /// </summary>
    public string GetEffectDescription(PileType pileType, Creature? target)
        => GetEffectDescription(pileType, target, IsUpgraded ? UpgradeDisplay.Upgraded : UpgradeDisplay.Normal);

    /// <summary>
    /// 升级感知的取描述:由 <see cref="EffectAttachmentModifier"/> 按宿主拼卡的升级/预览状态传入
    /// <paramref name="display"/>。效果卡是 canonical 单例、IsUpgraded 恒为 false,而拼接描述里的
    /// <c>{IfUpgraded:show:A|B}</c> 需要按宿主拼卡是否升级来渲染,所以不能直接走
    /// GetDescriptionForPile 的 IsUpgraded 判定(否则升级后的军械等效果卡永远显示基础分支)。
    /// </summary>
    public string GetEffectDescription(PileType pileType, Creature? target, UpgradeDisplay display)
        => BuildEffectDescription(pileType, target, display);

    /// <summary>
    /// 效果卡描述渲染:复刻 <see cref="GetDescriptionForPile(PileType, Creature?)"/> 的 LocString 拼接,
    /// 但 (1) UpgradeDisplay 由调用方显式指定(宿主升级态), (2) 不插入关键词行
    /// (效果卡的关键词已由拼卡逻辑转移到宿主上,由宿主自己的描述显示,避免重复),
    /// (3) 效果卡是 canonical 单例,Enchantment/Affliction/复播计数恒为空,一并省略。
    /// </summary>
    private string BuildEffectDescription(PileType pileType, Creature? target, UpgradeDisplay upgradeDisplay)
    {
        LocString description = Description;
        DynamicVars.AddTo(description);
        AddExtraArgsToDescription(description);
        description.Add(new IfUpgradedVar(upgradeDisplay));
        description.Add("OnTable", pileType is PileType.Hand or PileType.Play);
        description.Add("InCombat", CombatManager.Instance.IsInProgress && (Pile?.IsCombatPile ?? pileType.IsCombatPile()));
        description.Add("IsTargeting", target != null);
        description.Add("TargetType", TargetType.ToString());
        description.Add("GainsBlock", GainsBlock);
        description.Add("IsOstyAlive", IsMutable && (Owner?.IsOstyAlive ?? false));
        string prefix = EnergyIconHelper.GetPrefix(this);
        description.Add("energyPrefix", prefix);
        description.Add("singleStarIcon", "[img]res://images/packed/sprite_fonts/star_icon.png[/img]");
        foreach (KeyValuePair<string, object> kv in description.Variables)
        {
            if (kv.Value is EnergyVar energyVar)
            {
                energyVar.ColorPrefix = prefix;
            }
        }
        return description.GetFormattedText();
    }

    /// <summary>宿主拼卡当前给本效果带来的有效升级级数:未升级 = 0;升级后 = min(宿主等级, 本卡上限)。</summary>
    protected int EffectiveUpgradeLevels(CardModel hostCard)
        => hostCard.IsUpgraded ? Math.Min(hostCard.CurrentUpgradeLevel, MaxUpgradeLevels) : 0;

    /// <summary>
    /// 升级感知取值:宿主拼卡每升一级,在基础值上叠加一份 upgradeDelta。
    ///
    /// 为什么不在效果卡上 override OnUpgrade 直接改 DynamicVars 基础值:
    /// 效果卡是 ModelDb 里的共享单例(ResolveEffectCard → ModelDb.GetById 返回同一实例),
    /// 改它的 DynamicVars 会跨战斗、跨拼卡泄漏(同 Rampage 处理)。
    /// 所以升级增量一律在 ExecuteEffect 里按宿主拼卡的升级级数实时叠加。
    /// 增量来源 = partsmith_effect_cards.json 的 upgrade.vars(由生成脚本内联成常量)。
    /// 级数封顶 = <see cref="MaxUpgradeLevels"/>(默认 1,壳升多级也只吃一级)。
    /// </summary>
    protected decimal UpgradedValue(CardPlay cardPlay, decimal baseValue, decimal upgradeDelta)
        => baseValue + upgradeDelta * EffectiveUpgradeLevels(cardPlay.Card);

    /// <summary>同 <see cref="UpgradedValue"/>,用于 IntValue 取整型值的场景。</summary>
    protected int UpgradedIntValue(CardPlay cardPlay, int baseValue, int upgradeDelta)
        => baseValue + upgradeDelta * EffectiveUpgradeLevels(cardPlay.Card);

    /// <summary>
    /// 按 DynamicVar 名取"宿主拼卡升级"时该 var 的增量(仅展示用)。
    /// 由生成脚本按 partsmith_effect_cards.json 的 upgrade.vars 内联成 switch 常量。
    /// </summary>
    protected virtual decimal GetUpgradeDelta(string varName) => 0m;

    /// <summary>
    /// 用宿主拼卡的上下文(力量、目标易伤/虚弱、拼卡升级等)刷新本效果卡 DynamicVars 的预览值,
    /// 让拼卡描述里的 {Damage:diff()} 等占位符显示"打向该目标时"的实际数值。
    /// 仅在展示阶段调用(只改 PreviewValue/EnchantedValue,不改任何游戏状态,不改 BaseValue)。
    ///
    /// 为什么逐 var 直调 UpdateCardPreview 而不调 hostCard.UpdateDynamicVarPreview:
    /// 该方法被 BaseLib 的 UpdateModifierPreview transpiler 改写,会再次派发各 modifier 的预览,
    /// 若 modifier 再回调会造成递归/重复刷新(见 proposal_effect_card_dynamic_damage.md §3)。
    /// </summary>
    /// <param name="hostCard">拼接后的宿主卡(提供 Owner / CombatState / Pile / IsUpgraded 等上下文)。</param>
    /// <param name="target">当前选中的目标敌人;未选中时为 null。</param>
    public virtual void RefreshPreviewForHost(CardModel hostCard, Creature? target)
    {
        // runGlobalHooks 判定与 CardModel.UpdateDynamicVarPreview 完全一致:
        // 只在战斗中的手牌/打出区(或战斗内升级预览)才跑全局伤害/格挡 Hook(力量/易伤/虚弱)。
        // 卡组/地图等非战斗场景跑全局 Hook 拿不到 CombatState,行为不安全,回退为基础值。
        bool runGlobalHooks = hostCard.CombatState != null
            && (hostCard.Pile?.Type is PileType.Hand or PileType.Play
                || hostCard.UpgradePreviewType == CardUpgradePreviewType.Combat);
        int levels = EffectiveUpgradeLevels(hostCard);
        foreach (var v in DynamicVars.Values)
        {
            decimal delta = GetUpgradeDelta(v.Name) * levels;
            // 先清掉上一宿主/上次预览的残留预览值(DamageVar/BlockVar 等会重算 PreviewValue,
            // 但纯 DynamicVar 不重算,不清会残留上次升级宿主叠的增量)。无升级、非战斗时回到底值。
            v.PreviewValue = v.BaseValue;
            // 升级注入:先把"牌面基线" EnchantedValue 抬到 基础值+增量(效果卡没有 enchantment,
            // DamageVar.UpdateCardPreview 只在有 enchantment 时才会覆盖 EnchantedValue,这里不会被冲掉),
            // 再算预览,最后把增量加到 PreviewValue 上 → 升级后显示升级值且高亮比较以升级基线为基准。
            if (delta != 0m)
            {
                v.EnchantedValue = v.BaseValue + delta;
            }
            v.UpdateCardPreview(hostCard, CardPreviewMode.Normal, target, runGlobalHooks);
            if (delta != 0m)
            {
                v.PreviewValue += delta;
            }
        }
    }

    /// <summary>效果卡自身的 OnPlay = 跑一次效果脚本(自洽;正常流程里效果卡不会被直接打出)。</summary>
    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        => ExecuteEffect(choiceContext, cardPlay);
}
