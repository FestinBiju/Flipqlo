using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Flipqlo.Engine;

namespace Flipqlo.Rendering;

/// <summary>
/// Custom WPF element that renders the entire flip clock using DrawingContext.
/// This mirrors the Android Canvas approach for maximum visual parity.
/// </summary>
public sealed class FlipClockRenderer : FrameworkElement
{
    private readonly ClockEngine _clock;
    private readonly DispatcherTimer _tickTimer;

    // Per-digit animation state
    private readonly double[] _flipProgress; // 0.0 = idle, 0.0→1.0 = animating
    private readonly bool[] _isFlipping;
    private readonly int[] _flipOldDigit;
    private readonly int[] _flipNewDigit;
    private DateTime[] _flipStartTime;

    // Pre-built typeface
    private readonly Typeface _typeface;

    // Rendering flag
    private bool _isAnimating;

    // User scale factors
    private readonly double _scaleH;  // horizontal width multiplier
    private readonly double _scaleV;  // vertical height multiplier
    private readonly double _scaleAll; // overall multiplier

    public FlipClockRenderer(UserSettings settings)
    {
        _clock = new ClockEngine
        {
            Use24Hour = settings.Use24Hour,
            ShowSeconds = settings.ShowSeconds
        };

        _scaleH = settings.HorizontalScale;
        _scaleV = settings.VerticalScale;
        _scaleAll = settings.OverallScale;

        int maxDigits = 6;
        _flipProgress = new double[maxDigits];
        _isFlipping = new bool[maxDigits];
        _flipOldDigit = new int[maxDigits];
        _flipNewDigit = new int[maxDigits];
        _flipStartTime = new DateTime[maxDigits];

        _typeface = new Typeface(
            new FontFamily("Segoe UI, Arial, Helvetica"),
            FontStyles.Normal,
            FontWeights.Bold,
            FontStretches.Normal);

        // Tick timer: polls time every 250ms
        _tickTimer = new DispatcherTimer(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromMilliseconds(DesignTokens.TickIntervalMs)
        };
        _tickTimer.Tick += OnTimerTick;

        Loaded += (_, _) =>
        {
            _tickTimer.Start();
            InvalidateVisual();
        };
        Unloaded += (_, _) =>
        {
            _tickTimer.Stop();
            StopRenderLoop();
        };
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (!_clock.Tick())
            return;

        // Start flip animation for each changed digit
        var now = DateTime.Now;
        for (int i = 0; i < _clock.DigitCount; i++)
        {
            if (_clock.DigitChanged(i))
            {
                _isFlipping[i] = true;
                _flipOldDigit[i] = _clock.PreviousDigits[i];
                _flipNewDigit[i] = _clock.CurrentDigits[i];
                _flipProgress[i] = 0.0;
                _flipStartTime[i] = now;
            }
        }

        StartRenderLoop();
    }

    private void StartRenderLoop()
    {
        if (!_isAnimating)
        {
            _isAnimating = true;
            CompositionTarget.Rendering += OnFrame;
        }
    }

    private void StopRenderLoop()
    {
        if (_isAnimating)
        {
            _isAnimating = false;
            CompositionTarget.Rendering -= OnFrame;
        }
    }

    private void OnFrame(object? sender, EventArgs e)
    {
        var now = DateTime.Now;
        bool anyActive = false;

        for (int i = 0; i < _clock.DigitCount; i++)
        {
            if (!_isFlipping[i]) continue;

            double elapsed = (now - _flipStartTime[i]).TotalMilliseconds;
            _flipProgress[i] = Math.Clamp(elapsed / DesignTokens.FlipDurationMs, 0.0, 1.0);

            if (_flipProgress[i] >= 1.0)
            {
                _isFlipping[i] = false;
                _flipProgress[i] = 0.0;
            }
            else
            {
                anyActive = true;
            }
        }

        InvalidateVisual();

        if (!anyActive)
            StopRenderLoop();
    }

    protected override void OnRender(DrawingContext dc)
    {
        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0) return;

        // Fill background
        dc.DrawRectangle(DesignTokens.BackgroundBrush, null, new Rect(0, 0, w, h));

        // Calculate card dimensions with user scale applied
        double baseCardHeight = h * (DesignTokens.ClockToScreenPct / 100.0);
        double cardHeight = baseCardHeight * _scaleV * _scaleAll;
        double cardWidth = baseCardHeight * DesignTokens.CardAspectRatio * _scaleH * _scaleAll;
        double cornerRadius = cardHeight * (DesignTokens.CardCornerRadiusPct / 100.0);
        double interGap = cardHeight * (DesignTokens.InterDigitGapPct / 100.0);
        double groupGap = cardHeight * (DesignTokens.GroupGapPct / 100.0);
        double colonWidth = cardHeight * (DesignTokens.ColonWidthToCardPct / 100.0);

        int digitCount = _clock.DigitCount;
        int groups = digitCount / 2; // 2 or 3 groups

        // Total width of the clock assembly
        double totalWidth = digitCount * cardWidth
            + (groups - 1) * colonWidth           // colons between groups
            + (digitCount - groups) * interGap     // inter-digit gaps within groups
            + (groups - 1) * groupGap;             // extra space around colons
        // Simplify: groups of 2 digits each
        totalWidth = groups * (2 * cardWidth + interGap)
            + (groups - 1) * (groupGap + colonWidth + groupGap)
            - groups * interGap + (digitCount - groups) * interGap;

        // Recalculate more cleanly
        totalWidth = 0;
        for (int g = 0; g < groups; g++)
        {
            totalWidth += cardWidth + interGap + cardWidth; // two cards + gap
            if (g < groups - 1)
                totalWidth += groupGap + colonWidth + groupGap; // colon space
        }

        double startX = (w - totalWidth) / 2.0;
        double startY = h * DesignTokens.VerticalCenterBias - cardHeight / 2.0;

        double x = startX;

        for (int g = 0; g < groups; g++)
        {
            int d0 = g * 2;
            int d1 = g * 2 + 1;

            // First digit
            RenderDigitCard(dc, x, startY, cardWidth, cardHeight, cornerRadius, d0);
            x += cardWidth + interGap;

            // Second digit
            RenderDigitCard(dc, x, startY, cardWidth, cardHeight, cornerRadius, d1);
            x += cardWidth;

            // Colon (between groups, not after last)
            if (g < groups - 1)
            {
                x += groupGap;
                RenderColon(dc, x, startY, colonWidth, cardHeight);
                x += colonWidth + groupGap;
            }
        }
    }

    private void RenderDigitCard(DrawingContext dc, double x, double y,
        double w, double h, double cr, int digitIndex)
    {
        var bounds = new Rect(x, y, w, h);
        double halfH = h / 2.0;
        var topRect = new Rect(x, y, w, halfH);
        var bottomRect = new Rect(x, y + halfH, w, halfH);

        int currentDigit = _clock.CurrentDigits[digitIndex];
        int displayDigit = currentDigit;
        int oldDigit = currentDigit;
        double progress = 0.0;
        bool animating = _isFlipping[digitIndex];

        if (animating)
        {
            oldDigit = _flipOldDigit[digitIndex];
            displayDigit = _flipNewDigit[digitIndex];
            progress = _flipProgress[digitIndex];
        }

        string newStr = displayDigit.ToString();
        string oldStr = oldDigit.ToString();

        double fontSize = h * (DesignTokens.DigitToCardHeightPct / 100.0);

        // ── 1. Card background ──────────────────────────────────────────
        dc.DrawRoundedRectangle(DesignTokens.CardFaceBrush, null, bounds, cr, cr);

        if (!animating)
        {
            // Static state: draw full digit, divider
            DrawClippedDigit(dc, bounds, topRect, newStr, fontSize);
            DrawClippedDigit(dc, bounds, bottomRect, newStr, fontSize);
        }
        else
        {
            // ── 2. Static layers (behind the flaps) ─────────────────────

            // Top static: new digit top (revealed as top flap folds away)
            DrawClippedDigit(dc, bounds, topRect, newStr, fontSize);

            // Bottom static: old digit bottom (covered by bottom flap coming down)
            DrawClippedDigit(dc, bounds, bottomRect, oldStr, fontSize);

            // ── 3. Animated flaps ───────────────────────────────────────
            if (progress < 0.5)
            {
                // Top flap: old digit top, folding down
                double phase = progress / 0.5; // 0→1 within this phase
                double eased = EaseIn(phase);
                double scaleY = 1.0 - eased;

                dc.PushClip(new RectangleGeometry(topRect));
                dc.PushTransform(new ScaleTransform(1.0, scaleY, x + w / 2, y + halfH));

                // Flap background (slightly lighter to give depth)
                dc.DrawRectangle(DesignTokens.CardFaceBrush, null, topRect);
                DrawClippedDigit(dc, bounds, topRect, oldStr, fontSize);

                // Shadow overlay on the flap as it folds
                var shadowAlpha = eased * DesignTokens.FlapShadowMaxAlpha;
                var shadowBrush = new SolidColorBrush(Color.FromScRgb((float)shadowAlpha, 0, 0, 0));
                shadowBrush.Freeze();
                dc.DrawRectangle(shadowBrush, null, topRect);

                dc.Pop(); // transform
                dc.Pop(); // clip

                // Also draw bottom half with current (old) digit since flap hasn't arrived yet
                // (bottom static already shows old digit, which is correct)
            }
            else
            {
                // Bottom flap: new digit bottom, unfolding into place
                double phase = (progress - 0.5) / 0.5; // 0→1 within this phase
                double eased = EaseOut(phase);
                double scaleY = eased;

                dc.PushClip(new RectangleGeometry(bottomRect));
                dc.PushTransform(new ScaleTransform(1.0, scaleY, x + w / 2, y + halfH));

                // Flap background
                dc.DrawRectangle(DesignTokens.CardFaceBrush, null, bottomRect);
                DrawClippedDigit(dc, bounds, bottomRect, newStr, fontSize);

                // Shadow overlay, fading out as it settles
                var shadowAlpha = (1.0 - eased) * DesignTokens.FlapShadowMaxAlpha;
                var shadowBrush = new SolidColorBrush(Color.FromScRgb((float)shadowAlpha, 0, 0, 0));
                shadowBrush.Freeze();
                dc.DrawRectangle(shadowBrush, null, bottomRect);

                dc.Pop(); // transform
                dc.Pop(); // clip
            }
        }

        // ── 4. Divider line (always on top) ─────────────────────────────
        double divY = y + halfH;
        dc.DrawLine(DesignTokens.DividerShadowPen,
            new Point(x, divY + 1), new Point(x + w, divY + 1));
        dc.DrawLine(DesignTokens.DividerPen,
            new Point(x, divY), new Point(x + w, divY));
    }

    private void DrawClippedDigit(DrawingContext dc, Rect cardBounds, Rect clipRect,
        string digit, double fontSize)
    {
        dc.PushClip(new RectangleGeometry(clipRect));

        var ft = new FormattedText(
            digit,
            CultureInfo.InvariantCulture,
            FlowDirection.LeftToRight,
            _typeface,
            fontSize,
            DesignTokens.DigitBrush,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        // Center the text in the full card bounds (not clip bounds)
        double tx = cardBounds.X + (cardBounds.Width - ft.Width) / 2.0;
        double ty = cardBounds.Y + (cardBounds.Height - ft.Height) / 2.0;

        dc.DrawText(ft, new Point(tx, ty));
        dc.Pop(); // clip
    }

    private void RenderColon(DrawingContext dc, double x, double y,
        double colonW, double cardH)
    {
        double dotRadius = cardH * 0.035;
        double cx = x + colonW / 2.0;
        double cy1 = y + cardH * 0.35;
        double cy2 = y + cardH * 0.65;

        dc.DrawEllipse(DesignTokens.ColonBrush, null, new Point(cx, cy1), dotRadius, dotRadius);
        dc.DrawEllipse(DesignTokens.ColonBrush, null, new Point(cx, cy2), dotRadius, dotRadius);
    }

    // ── Easing functions ────────────────────────────────────────────────
    // Matching the behavior spec: ease-in for top flap, ease-out for bottom flap

    private static double EaseIn(double t)
    {
        // Quadratic ease-in: accelerating from zero velocity
        return t * t;
    }

    private static double EaseOut(double t)
    {
        // Quadratic ease-out: decelerating to zero velocity
        return 1.0 - (1.0 - t) * (1.0 - t);
    }
}
