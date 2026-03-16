package com.flipqlo.screensaver

import android.content.Context
import android.graphics.*
import android.os.Handler
import android.os.Looper
import android.util.AttributeSet
import android.view.Choreographer
import android.view.View
import androidx.preference.PreferenceManager

/**
 * Custom View that renders the flip clock using Canvas.
 * Mirrors Flipqlo.Rendering.FlipClockRenderer from the Windows codebase.
 *
 * Rendering logic is structurally identical to the WPF DrawingContext version
 * to guarantee visual parity across platforms.
 */
class FlipClockView @JvmOverloads constructor(
    context: Context,
    attrs: AttributeSet? = null,
    defStyleAttr: Int = 0
) : View(context, attrs, defStyleAttr), Choreographer.FrameCallback {

    // ── Settings ────────────────────────────────────────────────────────
    private val prefs = PreferenceManager.getDefaultSharedPreferences(context)
    private val clock = ClockEngine(
        use24Hour   = prefs.getBoolean("use_24_hour", true),
        showSeconds = prefs.getBoolean("show_seconds", false)
    )

    // User scale factors (50–200 stored as int, converted to 0.5–2.0)
    private val scaleH:   Float = prefs.getInt("horizontal_scale", 100) / 100f
    private val scaleV:   Float = prefs.getInt("vertical_scale", 100) / 100f
    private val scaleAll: Float = prefs.getInt("overall_scale", 100) / 100f

    // ── Per-digit animation state ───────────────────────────────────────
    private val flipProgress = FloatArray(6)
    private val isFlipping   = BooleanArray(6)
    private val flipOldDigit = IntArray(6)
    private val flipNewDigit = IntArray(6)
    private val flipStartNs  = LongArray(6)

    private var isAnimating = false

    // ── Paints (pre-allocated, matching DesignTokens) ───────────────────
    private val bgPaint = Paint().apply { color = DesignTokens.background; style = Paint.Style.FILL }
    private val cardPaint = Paint().apply {
        color = DesignTokens.cardFace; style = Paint.Style.FILL; isAntiAlias = true
    }
    private val digitPaint = Paint().apply {
        color = DesignTokens.digitColor; isAntiAlias = true
        typeface = Typeface.create("sans-serif", Typeface.BOLD)
        textAlign = Paint.Align.CENTER
    }
    private val colonPaint = Paint().apply {
        color = DesignTokens.colonColor; style = Paint.Style.FILL; isAntiAlias = true
    }
    private val dividerPaint = Paint().apply {
        color = DesignTokens.dividerLine; strokeWidth = 2f; style = Paint.Style.STROKE
    }
    private val dividerShadowPaint = Paint().apply {
        color = DesignTokens.dividerShadow; strokeWidth = 1f; style = Paint.Style.STROKE
    }
    private val flapShadowPaint = Paint().apply { style = Paint.Style.FILL }

    // ── Reusable drawing objects ────────────────────────────────────────
    private val tmpRect = RectF()
    private val tmpClip = RectF()

    // ── Tick timer ──────────────────────────────────────────────────────
    private val handler = Handler(Looper.getMainLooper())
    private val tickRunnable = object : Runnable {
        override fun run() {
            if (clock.tick()) {
                val now = System.nanoTime()
                for (i in 0 until clock.digitCount) {
                    if (clock.digitChanged(i)) {
                        isFlipping[i] = true
                        flipOldDigit[i] = clock.previousDigits[i]
                        flipNewDigit[i] = clock.currentDigits[i]
                        flipProgress[i] = 0f
                        flipStartNs[i] = now
                    }
                }
                startRenderLoop()
            }
            handler.postDelayed(this, DesignTokens.tickIntervalMs)
        }
    }

    // ── Lifecycle ───────────────────────────────────────────────────────

    override fun onAttachedToWindow() {
        super.onAttachedToWindow()
        handler.post(tickRunnable)
    }

    override fun onDetachedFromWindow() {
        super.onDetachedFromWindow()
        handler.removeCallbacks(tickRunnable)
        stopRenderLoop()
    }

    private fun startRenderLoop() {
        if (!isAnimating) {
            isAnimating = true
            Choreographer.getInstance().postFrameCallback(this)
        }
    }

    private fun stopRenderLoop() {
        isAnimating = false
    }

    override fun doFrame(frameTimeNanos: Long) {
        var anyActive = false
        for (i in 0 until clock.digitCount) {
            if (!isFlipping[i]) continue
            val elapsedMs = (frameTimeNanos - flipStartNs[i]) / 1_000_000f
            flipProgress[i] = (elapsedMs / DesignTokens.flipDurationMsF).coerceIn(0f, 1f)
            if (flipProgress[i] >= 1f) {
                isFlipping[i] = false
                flipProgress[i] = 0f
            } else {
                anyActive = true
            }
        }
        invalidate()
        if (anyActive) {
            Choreographer.getInstance().postFrameCallback(this)
        } else {
            isAnimating = false
        }
    }

    // ── Drawing ─────────────────────────────────────────────────────────

    override fun onDraw(canvas: Canvas) {
        val w = width.toFloat()
        val h = height.toFloat()
        if (w <= 0f || h <= 0f) return

        canvas.drawRect(0f, 0f, w, h, bgPaint)

        val baseCardHeight = h * DesignTokens.clockToScreenPct
        val cardHeight = baseCardHeight * scaleV * scaleAll
        val cardWidth  = baseCardHeight * DesignTokens.cardAspectRatio * scaleH * scaleAll
        val cr         = cardHeight * DesignTokens.cardCornerRadiusPct
        val interGap   = cardHeight * DesignTokens.interDigitGapPct
        val groupGap   = cardHeight * DesignTokens.groupGapPct
        val colonWidth = cardHeight * DesignTokens.colonWidthToCard

        val groups = clock.digitCount / 2

        digitPaint.textSize = cardHeight * DesignTokens.digitToCardHeight

        // Total width
        var totalWidth = 0f
        for (g in 0 until groups) {
            totalWidth += cardWidth + interGap + cardWidth
            if (g < groups - 1) totalWidth += groupGap + colonWidth + groupGap
        }

        var x = (w - totalWidth) / 2f
        val y = h * DesignTokens.verticalCenterBias - cardHeight / 2f

        for (g in 0 until groups) {
            val d0 = g * 2
            val d1 = g * 2 + 1

            drawDigitCard(canvas, x, y, cardWidth, cardHeight, cr, d0)
            x += cardWidth + interGap
            drawDigitCard(canvas, x, y, cardWidth, cardHeight, cr, d1)
            x += cardWidth

            if (g < groups - 1) {
                x += groupGap
                drawColon(canvas, x, y, colonWidth, cardHeight)
                x += colonWidth + groupGap
            }
        }
    }

    private fun drawDigitCard(
        canvas: Canvas, x: Float, y: Float,
        w: Float, h: Float, cr: Float, digitIndex: Int
    ) {
        val halfH = h / 2f
        val animating = isFlipping[digitIndex]

        val currentDigit = clock.currentDigits[digitIndex]
        val oldDigit: Int
        val newDigit: Int
        val progress: Float

        if (animating) {
            oldDigit = flipOldDigit[digitIndex]
            newDigit = flipNewDigit[digitIndex]
            progress = flipProgress[digitIndex]
        } else {
            oldDigit = currentDigit
            newDigit = currentDigit
            progress = 0f
        }

        val newStr = newDigit.toString()
        val oldStr = oldDigit.toString()

        // ── 1. Card background ──────────────────────────────────────────
        tmpRect.set(x, y, x + w, y + h)
        canvas.drawRoundRect(tmpRect, cr, cr, cardPaint)

        if (!animating) {
            // Static: draw full digit
            drawClippedDigit(canvas, x, y, w, h, x, y, w, halfH, newStr)
            drawClippedDigit(canvas, x, y, w, h, x, y + halfH, w, halfH, newStr)
        } else {
            // ── 2. Static layers behind flaps ───────────────────────────
            // Top: new digit top (revealed as old top flap folds)
            drawClippedDigit(canvas, x, y, w, h, x, y, w, halfH, newStr)
            // Bottom: old digit bottom (covered by new bottom flap)
            drawClippedDigit(canvas, x, y, w, h, x, y + halfH, w, halfH, oldStr)

            // ── 3. Animated flaps ───────────────────────────────────────
            if (progress < 0.5f) {
                // Top flap: old digit top, folding down
                val phase = progress / 0.5f
                val eased = easeIn(phase)
                val scaleY = 1f - eased

                canvas.save()
                tmpClip.set(x, y, x + w, y + halfH)
                canvas.clipRect(tmpClip)
                canvas.scale(1f, scaleY, x + w / 2f, y + halfH)

                // Flap card face
                tmpRect.set(x, y, x + w, y + halfH)
                canvas.drawRect(tmpRect, cardPaint)
                drawClippedDigit(canvas, x, y, w, h, x, y, w, halfH, oldStr)

                // Shadow overlay
                flapShadowPaint.color = Color.argb(
                    (eased * DesignTokens.flapShadowMaxAlpha * 255).toInt(), 0, 0, 0
                )
                canvas.drawRect(tmpRect, flapShadowPaint)

                canvas.restore()
            } else {
                // Bottom flap: new digit bottom, unfolding into place
                val phase = (progress - 0.5f) / 0.5f
                val eased = easeOut(phase)
                val scaleY = eased

                canvas.save()
                tmpClip.set(x, y + halfH, x + w, y + h)
                canvas.clipRect(tmpClip)
                canvas.scale(1f, scaleY, x + w / 2f, y + halfH)

                // Flap card face
                tmpRect.set(x, y + halfH, x + w, y + h)
                canvas.drawRect(tmpRect, cardPaint)
                drawClippedDigit(canvas, x, y, w, h, x, y + halfH, w, halfH, newStr)

                // Shadow overlay fading out
                flapShadowPaint.color = Color.argb(
                    ((1f - eased) * DesignTokens.flapShadowMaxAlpha * 255).toInt(), 0, 0, 0
                )
                canvas.drawRect(tmpRect, flapShadowPaint)

                canvas.restore()
            }
        }

        // ── 4. Divider line ─────────────────────────────────────────────
        val divY = y + halfH
        canvas.drawLine(x, divY + 1f, x + w, divY + 1f, dividerShadowPaint)
        canvas.drawLine(x, divY, x + w, divY, dividerPaint)
    }

    /**
     * Draws a digit string centered within cardBounds, but clipped to clipBounds.
     */
    private fun drawClippedDigit(
        canvas: Canvas,
        cardX: Float, cardY: Float, cardW: Float, cardH: Float,
        clipX: Float, clipY: Float, clipW: Float, clipH: Float,
        digit: String
    ) {
        canvas.save()
        tmpClip.set(clipX, clipY, clipX + clipW, clipY + clipH)
        canvas.clipRect(tmpClip)

        // Center text in full card bounds
        val textX = cardX + cardW / 2f
        val metrics = digitPaint.fontMetrics
        val textH = metrics.descent - metrics.ascent
        val textY = cardY + (cardH - textH) / 2f - metrics.ascent

        canvas.drawText(digit, textX, textY, digitPaint)
        canvas.restore()
    }

    private fun drawColon(canvas: Canvas, x: Float, y: Float, colonW: Float, cardH: Float) {
        val dotRadius = cardH * DesignTokens.colonDotRadiusPct
        val cx = x + colonW / 2f
        val cy1 = y + cardH * 0.35f
        val cy2 = y + cardH * 0.65f
        canvas.drawCircle(cx, cy1, dotRadius, colonPaint)
        canvas.drawCircle(cx, cy2, dotRadius, colonPaint)
    }

    // ── Easing (matching Windows version exactly) ───────────────────────

    private fun easeIn(t: Float): Float = t * t

    private fun easeOut(t: Float): Float = 1f - (1f - t) * (1f - t)
}
