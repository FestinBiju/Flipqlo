using System;
using Microsoft.Win32;

namespace FliqloScr;

public sealed class UserSettings
{
    private const string RegistryPath = @"SOFTWARE\FliqloReborn";

    public bool Use24Hour { get; set; } = true;
    public bool ShowSeconds { get; set; } = false;
    public bool PrimaryScreenOnly { get; set; } = false;

    // Scale factors: 50–200 stored as int (percent), exposed as double 0.5–2.0
    public int HorizontalScalePct { get; set; } = 100;
    public int VerticalScalePct { get; set; } = 100;
    public int OverallScalePct { get; set; } = 100;

    public double HorizontalScale => HorizontalScalePct / 100.0;
    public double VerticalScale => VerticalScalePct / 100.0;
    public double OverallScale => OverallScalePct / 100.0;

    public void Save()
    {
        using var key = Registry.CurrentUser.CreateSubKey(RegistryPath);
        key.SetValue("Use24Hour", Use24Hour ? 1 : 0, RegistryValueKind.DWord);
        key.SetValue("ShowSeconds", ShowSeconds ? 1 : 0, RegistryValueKind.DWord);
        key.SetValue("PrimaryScreenOnly", PrimaryScreenOnly ? 1 : 0, RegistryValueKind.DWord);
        key.SetValue("HorizontalScalePct", HorizontalScalePct, RegistryValueKind.DWord);
        key.SetValue("VerticalScalePct", VerticalScalePct, RegistryValueKind.DWord);
        key.SetValue("OverallScalePct", OverallScalePct, RegistryValueKind.DWord);
    }

    public static UserSettings Load()
    {
        var settings = new UserSettings();

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
            if (key != null)
            {
                settings.Use24Hour = ReadBool(key, "Use24Hour", true);
                settings.ShowSeconds = ReadBool(key, "ShowSeconds", false);
                settings.PrimaryScreenOnly = ReadBool(key, "PrimaryScreenOnly", false);
                settings.HorizontalScalePct = ReadInt(key, "HorizontalScalePct", 100);
                settings.VerticalScalePct = ReadInt(key, "VerticalScalePct", 100);
                settings.OverallScalePct = ReadInt(key, "OverallScalePct", 100);
            }
        }
        catch
        {
            // Return defaults on any registry error
        }

        return settings;
    }

    private static bool ReadBool(RegistryKey key, string name, bool defaultValue)
    {
        var val = key.GetValue(name);
        if (val is int i) return i != 0;
        return defaultValue;
    }

    private static int ReadInt(RegistryKey key, string name, int defaultValue)
    {
        var val = key.GetValue(name);
        if (val is int i) return Math.Clamp(i, 50, 200);
        return defaultValue;
    }
}
