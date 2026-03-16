using System;

namespace FliqloScr.Engine;

/// <summary>
/// Tracks current time and detects per-digit changes.
/// Shared logic — platform-independent algorithm.
/// </summary>
public sealed class ClockEngine
{
    private int[] _currentDigits;  // [H0, H1, M0, M1] or [H0, H1, M0, M1, S0, S1]
    private int[] _previousDigits;

    public bool Use24Hour { get; set; } = true;
    public bool ShowSeconds { get; set; } = false;

    public int DigitCount => ShowSeconds ? 6 : 4;

    /// <summary>Current digit values (0-9). Length is DigitCount.</summary>
    public ReadOnlySpan<int> CurrentDigits => _currentDigits.AsSpan(0, DigitCount);

    /// <summary>Previous digit values before last Tick(). Length is DigitCount.</summary>
    public ReadOnlySpan<int> PreviousDigits => _previousDigits.AsSpan(0, DigitCount);

    public ClockEngine()
    {
        _currentDigits = new int[6];
        _previousDigits = new int[6];
        ForceUpdate(DateTime.Now);
        Array.Copy(_currentDigits, _previousDigits, 6);
    }

    /// <summary>
    /// Checks the current time and updates digit arrays.
    /// Returns true if any visible digit changed.
    /// </summary>
    public bool Tick()
    {
        return Tick(DateTime.Now);
    }

    public bool Tick(DateTime now)
    {
        Array.Copy(_currentDigits, _previousDigits, 6);
        ForceUpdate(now);

        for (int i = 0; i < DigitCount; i++)
        {
            if (_currentDigits[i] != _previousDigits[i])
                return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if digit at index changed during the last Tick().
    /// </summary>
    public bool DigitChanged(int index)
    {
        if (index < 0 || index >= DigitCount) return false;
        return _currentDigits[index] != _previousDigits[index];
    }

    private void ForceUpdate(DateTime now)
    {
        int hour = Use24Hour ? now.Hour : (now.Hour % 12 == 0 ? 12 : now.Hour % 12);
        int min = now.Minute;
        int sec = now.Second;

        _currentDigits[0] = hour / 10;
        _currentDigits[1] = hour % 10;
        _currentDigits[2] = min / 10;
        _currentDigits[3] = min % 10;
        _currentDigits[4] = sec / 10;
        _currentDigits[5] = sec % 10;
    }
}
