using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Nodes;

namespace PartSmith.PartSmithCode.Rewards;

/// <summary>
/// 拼接流程里的临时浮动提示(类似 toast):顶部居中淡入淡出的一段文字。
/// 挂在 NRun.GlobalUi 下,不拦截鼠标;约 1.8 秒后自动消失。
/// </summary>
internal static class SpliceToast
{
    public static async Task Show(string text)
    {
        var ui = NRun.Instance?.GlobalUi;
        if (ui == null)
        {
            return;
        }

        var label = new Label
        {
            Text = text,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            AutowrapMode = TextServer.AutowrapMode.WordSmart,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        label.AddThemeFontSizeOverride("font_size", 26);
        label.AddThemeColorOverride("font_color", Colors.White);
        label.AddThemeColorOverride("font_outline_color", Colors.Black);
        label.AddThemeConstantOverride("outline_size", 8);

        // 顶部居中:锚点固定在顶边中点,偏移出一个 800x65 的框。
        label.AnchorLeft = 0.5f;
        label.AnchorRight = 0.5f;
        label.AnchorTop = 0f;
        label.AnchorBottom = 0f;
        label.OffsetLeft = -400f;
        label.OffsetRight = 400f;
        label.OffsetTop = 110f;
        label.OffsetBottom = 175f;

        label.Modulate = new Color(1f, 1f, 1f, 0f);
        ui.AddChild(label);

        try
        {
            var tween = label.CreateTween();
            tween.TweenProperty(label, "modulate:a", 1f, 0.15f);
            // ignoreCombatEnd=true:奖励阶段也不希望被跳过,提示该显示多久就显示多久。
            await Cmd.Wait(1.4f, ignoreCombatEnd: true);
            var fade = label.CreateTween();
            fade.TweenProperty(label, "modulate:a", 0f, 0.35f);
            await label.ToSignal(fade, Tween.SignalName.Finished);
        }
        finally
        {
            label.QueueFree();
        }
    }
}
