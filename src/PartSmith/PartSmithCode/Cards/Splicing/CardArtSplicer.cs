using System.Collections.Generic;
using System.Linq;
using Godot;
using PartSmith.PartSmithCode.Cards.Base;

namespace PartSmith.PartSmithCode.Cards.Splicing;

/// <summary>
/// 拼卡卡面合成器:把已拼效果卡的原版原画切片合成一张卡面 Texture2D。
/// 规则(用户定稿):1 张 = 原画整张;2 张 = 中间分开(左=第 1 张,右=第 2 张);
/// 3 张 = 三等分;≥4 张 = 维持三拼(取前 3)。
///
/// 切片源用原版卡独立原画 PNG(res://images/packed/card_portraits/…):
/// 纯图片,Texture2D.GetImage() 拿到整张原画,BlitRect 直接切;不用 atlas .tres
/// (自定义 sheet 子图,GetImage() 行为不确定)。个别卡(Blaze/Outrage)没有普通原画、
/// 只有 beta 原画,自动回退 beta PNG。
///
/// 整条渲染链路(CardModel.Portrait → NCard.UpdatePortrait)无需改动:
/// 由 CostCardModelBase.CustomPortrait 调用本类,BaseLib 的 CustomCardPortrait patch
/// 拦截 CardModel.Portrait 直接返回合成图。
/// </summary>
public static class CardArtSplicer
{
    private const int MaxSlices = 3;

    /// <summary>签名 → 合成图。签名 = 效果卡 Id.Entry 有序拼接;同一签名全局复用一张图。</summary>
    private static readonly Dictionary<string, Texture2D> Cache = new();

    /// <summary>
    /// 合成卡面。效果卡列表为空 → null;任何一张缺原画 → null(由调用方回退第一张效果卡卡面)。
    /// </summary>
    public static Texture2D? Build(IReadOnlyList<EffectCardModelBase> effects)
    {
        if (effects.Count == 0)
        {
            return null;
        }

        int n = Math.Min(effects.Count, MaxSlices);
        string key = string.Join(">", effects.Take(n).Select(e => e.Id.Entry));
        if (Cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var src = new List<Image>(n);
        Texture2D? firstTex = null;
        int w = 0, h = 0;
        foreach (var e in effects.Take(n))
        {
            string png = e.SplicePngPath;
            if (string.IsNullOrEmpty(png) || !ResourceLoader.Exists(png))
            {
                png = e.SpliceBetaPngPath; // Blaze/Outrage 只有 beta 原画
            }
            if (string.IsNullOrEmpty(png) || !ResourceLoader.Exists(png))
            {
                return null; // 无原画来源
            }
            var tex = ResourceLoader.Load<Texture2D>(png, null, ResourceLoader.CacheMode.Reuse);
            if (tex == null)
            {
                return null;
            }
            firstTex ??= tex;
            var img = tex.GetImage();
            if (img == null)
            {
                return null;
            }
            if (img.GetFormat() != Image.Format.Rgba8)
            {
                // BlitRect 要求两图同格式。原版卡原画多为不透明 RGB8(无 alpha),合成底是 RGBA8,
                // 不转会触发 "format != p_src->format" 而静默失败 → 卡面空白(实测踩坑,见 godot.log)。
                img.Convert(Image.Format.Rgba8);
            }
            if (src.Count == 0)
            {
                w = img.GetWidth();
                h = img.GetHeight();
            }
            else if (img.GetWidth() != w || img.GetHeight() != h)
            {
                img.Resize(w, h); // 防御:统一到首张尺寸,避免 BlitRect 越界
            }
            src.Add(img);
        }

        if (n == 1)
        {
            // 单张直接复用原画纹理本身,不必走合成。(循环已保证 firstTex 非 null)
            Cache[key] = firstTex!;
            return firstTex!;
        }

        var composite = Image.CreateEmpty(w, h, false, Image.Format.Rgba8);
        composite.Fill(new Color(0f, 0f, 0f, 0f)); // 透明底,防切片边界残留脏像素
        for (int i = 0; i < n; i++)
        {
            int x0 = w * i / n;
            int x1 = w * (i + 1) / n;
            composite.BlitRect(src[i], new Rect2I(x0, 0, x1 - x0, h), new Vector2I(x0, 0));
        }

        var result = ImageTexture.CreateFromImage(composite);
        Cache[key] = result;
        return result;
    }
}
