package com.flipqlo.screensaver

import android.service.dreams.DreamService
import android.view.View

/**
 * Android DreamService — the system screensaver / daydream.
 *
 * Users enable this via:
 *   Settings → Display → Screen saver → Flipqlo Screensaver
 *
 * The service creates a FlipClockView that fills the screen
 * with the same visual output as the Windows .scr version.
 */
class FlipClockDreamService : DreamService() {

    override fun onAttachedToWindow() {
        super.onAttachedToWindow()

        isInteractive = false           // exit on touch
        isFullscreen = true             // no status bar
        isScreenBright = false          // dim screen for AMOLED friendliness

        val clockView = FlipClockView(this)
        clockView.systemUiVisibility = (
            View.SYSTEM_UI_FLAG_FULLSCREEN
            or View.SYSTEM_UI_FLAG_HIDE_NAVIGATION
            or View.SYSTEM_UI_FLAG_IMMERSIVE_STICKY
            or View.SYSTEM_UI_FLAG_LAYOUT_FULLSCREEN
            or View.SYSTEM_UI_FLAG_LAYOUT_HIDE_NAVIGATION
            or View.SYSTEM_UI_FLAG_LAYOUT_STABLE
        )

        setContentView(clockView)
    }
}
