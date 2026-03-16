package com.flipqlo.screensaver

import java.util.Calendar

/**
 * Tracks current time and detects per-digit changes.
 * Mirrors Flipqlo.Engine.ClockEngine from the Windows codebase.
 */
class ClockEngine(
    var use24Hour: Boolean = true,
    var showSeconds: Boolean = false
) {
    val digitCount: Int get() = if (showSeconds) 6 else 4

    val currentDigits  = IntArray(6)
    val previousDigits = IntArray(6)

    init {
        forceUpdate(Calendar.getInstance())
        System.arraycopy(currentDigits, 0, previousDigits, 0, 6)
    }

    /** Returns true if any visible digit changed. */
    fun tick(): Boolean {
        System.arraycopy(currentDigits, 0, previousDigits, 0, 6)
        forceUpdate(Calendar.getInstance())

        for (i in 0 until digitCount) {
            if (currentDigits[i] != previousDigits[i]) return true
        }
        return false
    }

    fun digitChanged(index: Int): Boolean {
        if (index < 0 || index >= digitCount) return false
        return currentDigits[index] != previousDigits[index]
    }

    private fun forceUpdate(cal: Calendar) {
        val rawHour = cal.get(Calendar.HOUR_OF_DAY)
        val hour = if (use24Hour) rawHour else {
            val h = rawHour % 12
            if (h == 0) 12 else h
        }
        val min = cal.get(Calendar.MINUTE)
        val sec = cal.get(Calendar.SECOND)

        currentDigits[0] = hour / 10
        currentDigits[1] = hour % 10
        currentDigits[2] = min  / 10
        currentDigits[3] = min  % 10
        currentDigits[4] = sec  / 10
        currentDigits[5] = sec  % 10
    }
}
