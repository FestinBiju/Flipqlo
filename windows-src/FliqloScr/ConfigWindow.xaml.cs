using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FliqloScr.Rendering;

namespace FliqloScr;

public partial class ConfigWindow : Window
{
    private readonly UserSettings _settings;
    private bool _initialized;
    private FlipClockRenderer? _previewRenderer;
    private const double PreviewVirtualWidth = 1000;
    private const double PreviewVirtualHeight = 260;

    public ConfigWindow()
    {
        InitializeComponent();
        _settings = UserSettings.Load();

        chk24Hour.IsChecked = _settings.Use24Hour;
        chkSeconds.IsChecked = _settings.ShowSeconds;
        chkPrimary.IsChecked = _settings.PrimaryScreenOnly;

        sliderHorizontal.Value = _settings.HorizontalScalePct;
        sliderVertical.Value = _settings.VerticalScalePct;
        sliderOverall.Value = _settings.OverallScalePct;

        _initialized = true;
        UpdateSliderLabels();
        RefreshPreview();
    }

    private void OnSliderChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_initialized)
        {
            UpdateSliderLabels();
            RefreshPreview();
        }
    }

    private void OnOptionChanged(object sender, RoutedEventArgs e)
    {
        if (_initialized)
            RefreshPreview();
    }

    private void UpdateSliderLabels()
    {
        lblHorizontal.Text = $"{(int)sliderHorizontal.Value}%";
        lblVertical.Text = $"{(int)sliderVertical.Value}%";
        lblOverall.Text = $"{(int)sliderOverall.Value}%";
    }

    private void OnReset(object sender, RoutedEventArgs e)
    {
        sliderHorizontal.Value = 100;
        sliderVertical.Value = 100;
        sliderOverall.Value = 100;
        RefreshPreview();
    }

    private void OnOk(object sender, RoutedEventArgs e)
    {
        _settings.Use24Hour = chk24Hour.IsChecked == true;
        _settings.ShowSeconds = chkSeconds.IsChecked == true;
        _settings.PrimaryScreenOnly = chkPrimary.IsChecked == true;
        _settings.HorizontalScalePct = (int)sliderHorizontal.Value;
        _settings.VerticalScalePct = (int)sliderVertical.Value;
        _settings.OverallScalePct = (int)sliderOverall.Value;
        _settings.Save();
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private UserSettings BuildPreviewSettings()
    {
        return new UserSettings
        {
            Use24Hour = chk24Hour.IsChecked == true,
            ShowSeconds = chkSeconds.IsChecked == true,
            PrimaryScreenOnly = chkPrimary.IsChecked == true,
            HorizontalScalePct = (int)sliderHorizontal.Value,
            VerticalScalePct = (int)sliderVertical.Value,
            OverallScalePct = (int)sliderOverall.Value
        };
    }

    private void RefreshPreview()
    {
        if (!_initialized)
            return;

        var previewSettings = BuildPreviewSettings();
        _previewRenderer = new FlipClockRenderer(previewSettings)
        {
            Width = PreviewVirtualWidth,
            Height = PreviewVirtualHeight
        };

        previewHost.Child = new Viewbox
        {
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.Both,
            Child = _previewRenderer
        };
    }
}
