using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;
using PartSmith.PartSmithCode.Cards.Base;
using PartSmith.PartSmithCode.Cards.Splicing;

namespace PartSmith.PartSmithCode.Patches;

/// <summary>
/// 给卡面右上角加一个"点数"角标:
/// - 费用卡(含已拼接的拼卡):显示**剩余点数**(容量 - 已用)。
/// - 效果卡:显示**需求点数**(PointCost)。
/// - 其他卡:隐藏。
///
/// 位置镜像左上角能量图标:能量在 CardContainer 局部坐标 (-166..-102, -227..-163),
/// 卡中心 x=12 → 右上角 = (126..190, -227..-163),随卡片缩放一起缩放。
/// 挂 _Ready(建 Label)+ Reload(刷新文本/可见性,Model 变化或拼卡后都会走到)。
/// </summary>
[HarmonyPatch(typeof(NCard))]
internal static class NCardPointLabelPatch
{
    private const string LabelName = "PartSmithPointLabel";
    private const string LabelPath = "CardContainer/PartSmithPointLabel";

    [HarmonyPatch("_Ready")]
    [HarmonyPostfix]
    private static void AddPointLabel(NCard __instance)
    {
        if (__instance.GetNodeOrNull(LabelPath) != null)
        {
            return; // 池复用:已经加过
        }

        var container = __instance.GetNodeOrNull<Control>("CardContainer");
        if (container == null)
        {
            return;
        }

        var label = new Label
        {
            Name = LabelName,
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        // 镜像能量图标位置(卡中心 x=12 → 右上的对称矩形)
        label.OffsetLeft = 126f;
        label.OffsetTop = -227f;
        label.OffsetRight = 190f;
        label.OffsetBottom = -163f;

        // 样式参考能量标签(kreon_bold + 描边)
        label.AddThemeFontOverride("font", ResourceLoader.Load<Font>("res://themes/kreon_bold_shared.tres"));
        label.AddThemeFontSizeOverride("font_size", 30);
        label.AddThemeColorOverride("font_color", new Color(1f, 0.9647f, 0.8863f));
        label.AddThemeColorOverride("font_outline_color", Colors.Black);
        label.AddThemeColorOverride("font_shadow_color", new Color(0f, 0f, 0f, 0.2f));
        label.AddThemeConstantOverride("outline_size", 16);
        label.AddThemeConstantOverride("shadow_offset_x", 2);
        label.AddThemeConstantOverride("shadow_offset_y", 2);

        container.AddChild(label);
        UpdatePointLabel(__instance, label);
    }

    [HarmonyPatch("Reload")]
    [HarmonyPostfix]
    private static void RefreshPointLabel(NCard __instance)
    {
        var label = __instance.GetNodeOrNull<Label>(LabelPath);
        if (label != null)
        {
            UpdatePointLabel(__instance, label);
        }
    }

    private static void UpdatePointLabel(NCard card, Label label)
    {
        var model = card.Model;
        string text;
        bool visible;
        if (model is CostCardModelBase cost)
        {
            int remaining = cost.PointCapacity - SpliceController.UsedPoints(cost);
            text = remaining.ToString();
            visible = true;
        }
        else if (model is EffectCardModelBase effect)
        {
            text = effect.PointCost.ToString();
            visible = true;
        }
        else
        {
            text = "";
            visible = false;
        }

        if (label.Text != text || label.Visible != visible)
        {
            label.Text = text;
            label.Visible = visible;
        }
    }
}
