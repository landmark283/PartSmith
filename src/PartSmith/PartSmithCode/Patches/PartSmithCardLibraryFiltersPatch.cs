using System;
using System.Collections.Generic;
using System.Reflection;
using BaseLib.Utils;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using PartSmith.PartSmithCode.Characters;
using PartSmith.PartSmithCode.Pools;

namespace PartSmith.PartSmithCode.Patches;

/// <summary>
/// 百科大全(CardLibrary)补两个卡池过滤器按钮:费用卡池 / 效果卡池。
///
/// 原版图鉴只给「角色」生成自定义池过滤器(BaseLib.CustomPoolFilters 按 CustomCharacterModel.CardPool
/// 过滤,而 BigWarrior.CardPool 暂指 IroncladCardPool);我们的费用/效果牌挂在两个共享自定义池上,
/// 没有任何过滤器按钮 → 即使 <c>ShouldShowInCardLibrary = true</c> 也点不到。
///
/// 本 patch 在 <see cref="NCardLibrary._Ready"/> 之后追加两个单选按钮(c.Pool is PartSmithCostCardPool /
/// PartSmithEffectCardPool),并让 BigWarrior 的局内打开图鉴默认选中「费用卡池」。
/// 复刻 BaseLib 的按钮构建(GenerateFilter 是私有静态方法,不能直接调)。
/// </summary>
[HarmonyPatch(typeof(NCardLibrary), "_Ready")]
internal static class PartSmithCardLibraryFiltersPatch
{
    private static readonly MethodInfo UpdateCardPoolFilterMethod =
        AccessTools.DeclaredMethod(typeof(NCardLibrary), "UpdateCardPoolFilter");

    private static readonly FieldInfo LastHoveredControlField =
        AccessTools.DeclaredField(typeof(NCardLibrary), "_lastHoveredControl");

    private const string CostFilterName = "FILTER-partsmith_cost";
    private const string EffectFilterName = "FILTER-partsmith_effect";
    private const string HunterCostFilterName = "FILTER-partsmith_hunter_cost";
    private const string HunterEffectFilterName = "FILTER-partsmith_hunter_effect";
    private const string WangCostFilterName = "FILTER-partsmith_wang_cost";
    private const string WangEffectFilterName = "FILTER-partsmith_wang_effect";
    private const string BoneManCostFilterName = "FILTER-partsmith_boneman_cost";
    private const string BoneManEffectFilterName = "FILTER-partsmith_boneman_effect";

    /// <summary>
    /// 必须用最高优先级(后置补丁里先跑,已实证本版本 Harmony postfix 按优先级降序执行),
    /// 让本 patch 先把按钮加进网格,再由 BaseLib 的 <c>AdjustFilterScales</c>(默认优先级)统一缩放,按钮大小才一致。
    /// </summary>
    [HarmonyPriority(Priority.First)]
    [HarmonyPostfix]
    private static void AddPartSmithPoolFilters(NCardLibrary __instance)
    {
        var poolFilters = AccessTools.DeclaredField(typeof(NCardLibrary), "_poolFilters").GetValue(__instance)
            as Dictionary<NCardPoolFilter, Func<CardModel, bool>>;
        var miscFilter = AccessTools.DeclaredField(typeof(NCardLibrary), "_miscPoolFilter").GetValue(__instance)
            as NCardPoolFilter;
        if (poolFilters == null || miscFilter == null)
        {
            return;
        }

        // 防重复(图鉴场景重开时 _Ready 会再跑一次)。
        foreach (var key in poolFilters.Keys)
        {
            if (key.Name.ToString() is CostFilterName or EffectFilterName or HunterCostFilterName or HunterEffectFilterName
                or WangCostFilterName or WangEffectFilterName or BoneManCostFilterName or BoneManEffectFilterName)
            {
                return;
            }
        }

        var costFilter = AddFilter(
            __instance, poolFilters, miscFilter, CostFilterName,
            new Color("8E7CC3"), new LocString("card_library", "PARTSMITH_POOL_COST_TIP"),
            (CardModel c) => c.Pool is PartSmithCostCardPool);
        AddFilter(
            __instance, poolFilters, costFilter, EffectFilterName,
            new Color("7CC38E"), new LocString("card_library", "PARTSMITH_POOL_EFFECT_TIP"),
            (CardModel c) => c.Pool is PartSmithEffectCardPool);

        // 小猎人专属池(silent 绿)过滤器按钮。
        var hunterCostFilter = AddFilter(
            __instance, poolFilters, miscFilter, HunterCostFilterName,
            new Color("1A6625"), new LocString("card_library", "PARTSMITH_POOL_HUNTER_COST_TIP"),
            (CardModel c) => c.Pool is PartSmithHunterCostCardPool);
        AddFilter(
            __instance, poolFilters, hunterCostFilter, HunterEffectFilterName,
            new Color("4C9E5A"), new LocString("card_library", "PARTSMITH_POOL_HUNTER_EFFECT_TIP"),
            (CardModel c) => c.Pool is PartSmithHunterEffectCardPool);

        // 王(储君)专属池(regent 橙)过滤器按钮。
        var wangCostFilter = AddFilter(
            __instance, poolFilters, miscFilter, WangCostFilterName,
            new Color("E36600"), new LocString("card_library", "PARTSMITH_POOL_WANG_COST_TIP"),
            (CardModel c) => c.Pool is PartSmithWangCostCardPool);
        AddFilter(
            __instance, poolFilters, wangCostFilter, WangEffectFilterName,
            new Color("FF9E2C"), new LocString("card_library", "PARTSMITH_POOL_WANG_EFFECT_TIP"),
            (CardModel c) => c.Pool is PartSmithWangEffectCardPool);

        // 骨头人专属池(necrobinder 粉)过滤器按钮。
        var boneManCostFilter = AddFilter(
            __instance, poolFilters, miscFilter, BoneManCostFilterName,
            new Color("CD4EED"), new LocString("card_library", "PARTSMITH_POOL_BONEMAN_COST_TIP"),
            (CardModel c) => c.Pool is PartSmithBoneManCostCardPool);
        AddFilter(
            __instance, poolFilters, boneManCostFilter, BoneManEffectFilterName,
            new Color("EE82EE"), new LocString("card_library", "PARTSMITH_POOL_BONEMAN_EFFECT_TIP"),
            (CardModel c) => c.Pool is PartSmithBoneManEffectCardPool);

        // 大战士局内打开图鉴默认选中「费用卡池」;小猎人默认选中「猎人费用卡池」;王默认选中「王费用卡池」;骨头人默认选中「骨头人费用卡池」。
        if (AccessTools.DeclaredField(typeof(NCardLibrary), "_cardPoolFilters").GetValue(__instance)
            is Dictionary<CharacterModel, NCardPoolFilter> cardPoolFilters)
        {
            cardPoolFilters[ModelDb.Character<BigWarrior>()] = costFilter;
            cardPoolFilters[ModelDb.Character<LittleHunter>()] = hunterCostFilter;
            cardPoolFilters[ModelDb.Character<Wang>()] = wangCostFilter;
            cardPoolFilters[ModelDb.Character<BoneMan>()] = boneManCostFilter;
        }
    }

    /// <summary>
    /// 构建一个池过滤器按钮并注册进 <c>_poolFilters</c>。节点结构(Image/Shadow/SelectionReticle)
    /// 与 BaseLib.CustomPoolFilters.GenerateFilter 一致,只是图标换成按池配色的纯色块。
    /// </summary>
    private static NCardPoolFilter AddFilter(
        NCardLibrary library,
        Dictionary<NCardPoolFilter, Func<CardModel, bool>> poolFilters,
        Node siblingBefore,
        string name,
        Color color,
        LocString? loc,
        Func<CardModel, bool> predicate)
    {
        var filter = new NCardPoolFilter
        {
            Name = name,
            Size = new Vector2(64f, 64f),
            CustomMinimumSize = new Vector2(64f, 64f),
            FocusMode = Control.FocusModeEnum.All,
            Loc = loc
        };

        Texture2D icon = SolidColorTexture(color);
        var image = new TextureRect
        {
            Name = "Image",
            Texture = icon,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Size = new Vector2(56f, 56f),
            Position = new Vector2(4f, 4f),
            Scale = new Vector2(0.9f, 0.9f),
            PivotOffset = new Vector2(28f, 28f),
            Material = ShaderUtils.GenerateHsv(1f, 1f, 1f)
        };
        var shadow = new TextureRect
        {
            Name = "Shadow",
            Texture = icon,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Size = new Vector2(56f, 56f),
            Position = new Vector2(4f, 3f),
            PivotOffset = new Vector2(28f, 28f),
            ShowBehindParent = true
        };
        Color black = Colors.Black;
        black.A = 0.25f;
        shadow.Modulate = black;
        image.AddChild(shadow, forceReadableName: false, Node.InternalMode.Disabled);

        var reticle = PreloadManager.Cache.GetScene(SceneHelper.GetScenePath("ui/selection_reticle"))
            .Instantiate<NSelectionReticle>(PackedScene.GenEditState.Disabled);
        reticle.Name = "SelectionReticle";
        reticle.UniqueNameInOwner = true;

        filter.AddChild(image, forceReadableName: false, Node.InternalMode.Disabled);
        image.Owner = filter;
        filter.AddChild(reticle, forceReadableName: false, Node.InternalMode.Disabled);
        reticle.Owner = filter;

        siblingBefore.AddSibling(filter, forceReadableName: true);
        poolFilters.Add(filter, predicate);
        filter.Connect(
            NCardPoolFilter.SignalName.Toggled,
            Callable.From<NCardPoolFilter>(f => UpdateCardPoolFilterMethod.Invoke(library, new object[] { f })));
        filter.Connect(
            Control.SignalName.FocusEntered,
            Callable.From(() => LastHoveredControlField.SetValue(library, filter)));

        return filter;
    }

    private static Texture2D SolidColorTexture(Color color)
    {
        Image image = Image.CreateEmpty(56, 56, false, Image.Format.Rgba8);
        image.Fill(color);
        return ImageTexture.CreateFromImage(image);
    }
}
