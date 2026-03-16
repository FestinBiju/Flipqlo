using System;
using System.Windows;
using System.Windows.Media;

namespace Flipqlo.Engine;

/// <summary>
/// Platform-agnostic design constants derived from shared/design-tokens.json.
/// Defined as code constants for zero-allocation access during rendering.
/// </summary>
public static class DesignTokens
{
    // ── Colors ──────────────────────────────────────────────────────────
    public static readonly Color Background      = ColorFrom(0xFF0A0A0A);
    public static readonly Color CardFace         = ColorFrom(0xFF1C1C1C);
    public static readonly Color CardHighlight    = ColorFrom(0xFF242424);
    public static readonly Color DigitColor       = ColorFrom(0xFFD8D8D8);
    public static readonly Color DividerLine      = ColorFrom(0xFF0F0F0F);
    public static readonly Color DividerShadow    = ColorFrom(0xFF000000);
    public static readonly Color ColonColor       = ColorFrom(0xFF3A3A3A);
    public static readonly Color FlapShadowColor  = ColorFrom(0x59000000); // ~35% alpha

    // ── Pre-built frozen brushes ────────────────────────────────────────
    public static readonly Brush BackgroundBrush  = Freeze(new SolidColorBrush(Background));
    public static readonly Brush CardFaceBrush    = Freeze(new SolidColorBrush(CardFace));
    public static readonly Brush CardHighlightBrush = Freeze(new SolidColorBrush(CardHighlight));
    public static readonly Brush DigitBrush       = Freeze(new SolidColorBrush(DigitColor));
    public static readonly Brush ColonBrush       = Freeze(new SolidColorBrush(ColonColor));
    public static readonly Brush FlapShadowBrush  = Freeze(new SolidColorBrush(FlapShadowColor));
    public static readonly Pen DividerPen         = FreezePen(new Pen(new SolidColorBrush(DividerLine), 2.0));
    public static readonly Pen DividerShadowPen   = FreezePen(new Pen(new SolidColorBrush(DividerShadow), 1.0));

    // ── Animation ───────────────────────────────────────────────────────
    public const double FlipDurationMs       = 600.0;
    public const double FlipTopPhaseRatio    = 0.5;
    public const double TickIntervalMs       = 250.0;
    public const double FlapShadowMaxAlpha   = 0.35;

    // ── Layout (ratios of card height unless noted) ─────────────────────
    public const double CardCornerRadiusPct  = 4.0;   // % of card height
    public const double CardAspectRatio      = 0.75;  // width / height
    public const double DigitToCardHeightPct = 68.0;
    public const double ColonWidthToCardPct  = 25.0;
    public const double InterDigitGapPct     = 3.0;
    public const double GroupGapPct          = 6.0;
    public const double ClockToScreenPct     = 32.0;
    public const double VerticalCenterBias   = 0.5;

    // ── Helpers ─────────────────────────────────────────────────────────
    private static Color ColorFrom(uint argb)
    {
        return Color.FromArgb(
            (byte)(argb >> 24),
            (byte)(argb >> 16),
            (byte)(argb >> 8),
            (byte)(argb));
    }

    private static Brush Freeze(Brush b) { b.Freeze(); return b; }
    private static Pen FreezePen(Pen p)
    {
        if (p.Brush is Freezable fb) fb.Freeze();
        p.Freeze();
        return p;
    }
}
