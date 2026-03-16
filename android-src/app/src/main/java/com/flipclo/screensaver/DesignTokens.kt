package com.fliqlo.screensaver

import android.graphics.Color

/**
 * Design constants matching the Windows version exactly.
 * Mirrors FliqloScr.Engine.DesignTokens from the Windows codebase.
 */
object DesignTokens {
    // ── Colors ──────────────────────────────────────────────────────────
    val background:      Int = 0xFF0A0A0A.toInt()
    val cardFace:        Int = 0xFF1C1C1C.toInt()
    val cardHighlight:   Int = 0xFF242424.toInt()
    val digitColor:      Int = 0xFFD8D8D8.toInt()
    val dividerLine:     Int = 0xFF0F0F0F.toInt()
    val dividerShadow:   Int = 0xFF000000.toInt()
    val colonColor:      Int = 0xFF3A3A3A.toInt()

    // ── Animation ───────────────────────────────────────────────────────
    const val flipDurationMs:     Long   = 600L
    const val flipDurationMsF:    Float  = 600f
    const val tickIntervalMs:     Long   = 250L
    const val flapShadowMaxAlpha: Float  = 0.35f

    // ── Layout (ratios of card height unless noted) ─────────────────────
    const val cardCornerRadiusPct: Float = 0.04f   // fraction of card height
    const val cardAspectRatio:     Float = 0.75f   // width / height
    const val digitToCardHeight:   Float = 0.68f
    const val colonWidthToCard:    Float = 0.25f
    const val interDigitGapPct:    Float = 0.03f
    const val groupGapPct:         Float = 0.06f
    const val clockToScreenPct:    Float = 0.32f
    const val verticalCenterBias:  Float = 0.5f
    const val colonDotRadiusPct:   Float = 0.035f
}
