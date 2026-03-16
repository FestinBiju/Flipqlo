using System;
using System.Globalization;
using System.Windows;
using System.Windows.Interop;
using FliqloScr.Native;

namespace FliqloScr;

public static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        Win32.SetProcessDPIAware();

        var app = new Application();
        app.ShutdownMode = ShutdownMode.OnMainWindowClose;

        var mode = ParseMode(args);

        switch (mode)
        {
            case ScreensaverMode.FullScreen:
                LaunchFullScreen(app);
                break;

            case ScreensaverMode.Preview:
                var hwnd = ParsePreviewHandle(args);
                LaunchPreview(app, hwnd);
                break;

            case ScreensaverMode.Config:
                LaunchConfig(app, ParseConfigOwnerHandle(args));
                break;
        }
    }

    private static ScreensaverMode ParseMode(string[] args)
    {
        if (args.Length == 0)
            return ScreensaverMode.Config;

        var arg = args[0].ToLower(CultureInfo.InvariantCulture).Trim();

        if (arg.StartsWith("/s", StringComparison.Ordinal) || arg.StartsWith("-s", StringComparison.Ordinal))
            return ScreensaverMode.FullScreen;

        if (arg.StartsWith("/p", StringComparison.Ordinal) || arg.StartsWith("-p", StringComparison.Ordinal))
            return ScreensaverMode.Preview;

        if (arg.StartsWith("/c", StringComparison.Ordinal) || arg.StartsWith("-c", StringComparison.Ordinal))
            return ScreensaverMode.Config;

        return ScreensaverMode.Config;
    }

    private static IntPtr ParsePreviewHandle(string[] args)
    {
        string? handleStr = null;

        if (args.Length >= 2)
        {
            handleStr = args[1];
        }
        else if (args[0].Contains(':'))
        {
            handleStr = args[0].Split(':')[1];
        }
        else if (args[0].Contains('='))
        {
            handleStr = args[0].Split('=')[1];
        }

        if (handleStr != null && long.TryParse(handleStr, out var h))
            return new IntPtr(h);

        return IntPtr.Zero;
    }

    private static IntPtr ParseConfigOwnerHandle(string[] args)
    {
        string? handleStr = null;

        if (args.Length >= 2)
        {
            handleStr = args[1];
        }
        else if (args.Length > 0 && args[0].Contains(':'))
        {
            handleStr = args[0].Split(':')[1];
        }
        else if (args.Length > 0 && args[0].Contains('='))
        {
            handleStr = args[0].Split('=')[1];
        }

        if (handleStr != null && long.TryParse(handleStr, out var h))
            return new IntPtr(h);

        return IntPtr.Zero;
    }

    private static void LaunchFullScreen(Application app)
    {
        var settings = UserSettings.Load();
        var screens = Win32.GetAllScreens();

        Window? primary = null;

        foreach (var screen in screens)
        {
            if (settings.PrimaryScreenOnly && !screen.IsPrimary)
                continue;

            var window = new ScreensaverWindow(settings);
            window.SetBounds(screen.Left, screen.Top, screen.Width, screen.Height);

            if (primary == null)
            {
                primary = window;
                app.MainWindow = window;
            }

            window.Show();
        }

        if (primary != null)
            app.Run();
    }

    private static void LaunchPreview(Application app, IntPtr parentHwnd)
    {
        if (parentHwnd == IntPtr.Zero)
            return;

        Win32.GetClientRect(parentHwnd, out var rect);
        var width = rect.Right - rect.Left;
        var height = rect.Bottom - rect.Top;

        var window = new ScreensaverWindow(UserSettings.Load(), isPreview: true);
        window.WindowStyle = WindowStyle.None;
        window.ResizeMode = ResizeMode.NoResize;
        window.ShowInTaskbar = false;
        window.Width = width;
        window.Height = height;

        window.Loaded += (_, _) =>
        {
            var helper = new WindowInteropHelper(window);
            Win32.SetParent(helper.Handle, parentHwnd);

            var style = Win32.GetWindowLong(helper.Handle, Win32.GWL_STYLE);
            style = (style | Win32.WS_CHILD) & ~Win32.WS_POPUP;
            Win32.SetWindowLong(helper.Handle, Win32.GWL_STYLE, style);

            Win32.MoveWindow(helper.Handle, 0, 0, width, height, true);
        };

        app.MainWindow = window;
        window.Show();
        app.Run();
    }

    private static void LaunchConfig(Application app, IntPtr ownerHwnd)
    {
        var window = new ConfigWindow();

        if (ownerHwnd != IntPtr.Zero)
        {
            window.SourceInitialized += (_, _) =>
            {
                var helper = new WindowInteropHelper(window)
                {
                    Owner = ownerHwnd
                };
            };
        }

        app.MainWindow = window;
        app.Run(window);
    }
}

public enum ScreensaverMode
{
    FullScreen,
    Preview,
    Config
}
