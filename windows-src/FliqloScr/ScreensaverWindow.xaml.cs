using System;
using System.Windows;
using System.Windows.Input;
using FliqloScr.Native;
using FliqloScr.Rendering;

namespace FliqloScr;

public partial class ScreensaverWindow : Window
{
    private readonly FlipClockRenderer _renderer;
    private readonly bool _isPreview;
    private Point? _initialMousePos;
    private const double MouseMoveThreshold = 10.0;

    public ScreensaverWindow(UserSettings settings, bool isPreview = false)
    {
        InitializeComponent();

        _isPreview = isPreview;
        _renderer = new FlipClockRenderer(settings);
        Content = _renderer;

        if (!isPreview)
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
            ShowInTaskbar = false;

            MouseMove += OnMouseMove;
            MouseDown += OnInputExit;
            KeyDown += OnInputExit;
        }
    }

    public void SetBounds(int left, int top, int width, int height)
    {
        Left = left;
        Top = top;
        Width = width;
        Height = height;
        WindowState = WindowState.Normal;
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        var pos = e.GetPosition(this);

        if (_initialMousePos == null)
        {
            _initialMousePos = pos;
            return;
        }

        var delta = pos - _initialMousePos.Value;
        if (delta.Length > MouseMoveThreshold)
        {
            CloseAll();
        }
    }

    private void OnInputExit(object sender, EventArgs e)
    {
        CloseAll();
    }

    private void CloseAll()
    {
        foreach (Window w in Application.Current.Windows)
        {
            w.Close();
        }
    }
}
