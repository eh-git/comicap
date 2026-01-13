using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace ComiCap;

/// <summary>
/// SlideCaptureSettingsWindow.xaml の相互作用ロジック
/// </summary>
public partial class SlideCaptureSettingsWindow : Window
{
    private readonly SlideCaptureSettings _settings;

    /// <summary>
    /// 設定を取得
    /// </summary>
    public SlideCaptureSettings Settings => _settings;

    /// <summary>
    /// コンストラクタ（既存の設定から初期化）
    /// </summary>
    public SlideCaptureSettingsWindow(AppSettings appSettings)
    {
        InitializeComponent();
        _settings = new SlideCaptureSettings(appSettings);
        LoadSettings();
    }

    /// <summary>
    /// 設定をUIに読み込み
    /// </summary>
    private void LoadSettings()
    {
        UrlTextBox.Text = _settings.Url;

        // ページめくりキーを選択
        foreach (ComboBoxItem item in PageTurnKeyComboBox.Items)
        {
            if (item.Tag.ToString() == _settings.PageTurnKey.ToString())
            {
                PageTurnKeyComboBox.SelectedItem = item;
                break;
            }
        }

        CaptureIntervalSlider.Value = _settings.CaptureIntervalMs;
        CaptureIntervalTextBox.Text = _settings.CaptureIntervalMs.ToString();
        CaptureCountTextBox.Text = _settings.CaptureCount.ToString();
        SaveDirectoryTextBox.Text = _settings.SaveDirectory;

        // 画像形式を選択
        foreach (ComboBoxItem item in ImageFormatComboBox.Items)
        {
            if (item.Tag.ToString() == _settings.ImageFormat.ToString())
            {
                ImageFormatComboBox.SelectedItem = item;
                break;
            }
        }

        JpegQualitySlider.Value = _settings.JpegQuality;
        FileNamePrefixTextBox.Text = _settings.FileNamePrefix;
        BrowserPathTextBox.Text = _settings.BrowserPath;
        MaximizeBrowserCheckBox.IsChecked = _settings.MaximizeBrowser;

        UpdateJpegQualityVisibility();
    }

    /// <summary>
    /// UIから設定を保存
    /// </summary>
    private void SaveSettings()
    {
        _settings.Url = UrlTextBox.Text.Trim();

        // ページめくりキーを取得
        if (PageTurnKeyComboBox.SelectedItem is ComboBoxItem selectedKey)
        {
            _settings.PageTurnKey = Enum.Parse<PageTurnKey>(selectedKey.Tag.ToString()!);
        }

        _settings.CaptureIntervalMs = int.Parse(CaptureIntervalTextBox.Text);
        _settings.CaptureCount = int.Parse(CaptureCountTextBox.Text);
        _settings.SaveDirectory = SaveDirectoryTextBox.Text;

        // 画像形式を取得
        if (ImageFormatComboBox.SelectedItem is ComboBoxItem selectedFormat)
        {
            _settings.ImageFormat = Enum.Parse<ImageFormat>(selectedFormat.Tag.ToString()!);
        }

        _settings.JpegQuality = (int)JpegQualitySlider.Value;
        _settings.FileNamePrefix = FileNamePrefixTextBox.Text.Trim();
        _settings.BrowserPath = BrowserPathTextBox.Text.Trim();
        _settings.MaximizeBrowser = MaximizeBrowserCheckBox.IsChecked ?? true;
    }

    /// <summary>
    /// JPEG品質パネルの表示/非表示を更新
    /// </summary>
    private void UpdateJpegQualityVisibility()
    {
        if (ImageFormatComboBox.SelectedItem is ComboBoxItem item)
        {
            var isJpeg = item.Tag.ToString() == "JPEG";
            JpegQualityLabel.Visibility = isJpeg ? Visibility.Visible : Visibility.Collapsed;
            JpegQualityPanel.Visibility = isJpeg ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    /// <summary>
    /// キャプチャ間隔スライダーの値変更
    /// </summary>
    private void CaptureIntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (CaptureIntervalTextBox != null)
        {
            var value = (int)e.NewValue;
            CaptureIntervalTextBox.Text = value.ToString();
            if (CaptureIntervalLabel != null)
            {
                CaptureIntervalLabel.Text = $"待機時間: {value / 1000.0:F1}秒";
            }
        }
    }

    /// <summary>
    /// キャプチャ間隔テキストボックスの値変更
    /// </summary>
    private void CaptureIntervalTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (CaptureIntervalSlider != null && int.TryParse(CaptureIntervalTextBox.Text, out var value))
        {
            if (value >= 100 && value <= 10000)
            {
                CaptureIntervalSlider.Value = value;
            }
        }
    }

    /// <summary>
    /// 画像形式選択変更
    /// </summary>
    private void ImageFormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateJpegQualityVisibility();
    }

    /// <summary>
    /// JPEG品質スライダーの値変更
    /// </summary>
    private void JpegQualitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (JpegQualityValueLabel != null)
        {
            JpegQualityValueLabel.Text = ((int)e.NewValue).ToString();
        }
    }

    /// <summary>
    /// 保存先フォルダ参照ボタンクリック
    /// </summary>
    private void BrowseSaveDirectory_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "保存先フォルダを選択してください",
            SelectedPath = SaveDirectoryTextBox.Text,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            SaveDirectoryTextBox.Text = dialog.SelectedPath;
        }
    }

    /// <summary>
    /// ブラウザパス参照ボタンクリック
    /// </summary>
    private void BrowseBrowserPath_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "ブラウザの実行ファイルを選択",
            Filter = "実行ファイル (*.exe)|*.exe|すべてのファイル (*.*)|*.*",
            CheckFileExists = true
        };

        if (!string.IsNullOrWhiteSpace(BrowserPathTextBox.Text))
        {
            dialog.FileName = BrowserPathTextBox.Text;
        }

        if (dialog.ShowDialog() == true)
        {
            BrowserPathTextBox.Text = dialog.FileName;
        }
    }

    /// <summary>
    /// 開始ボタンクリック
    /// </summary>
    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 設定を保存
            SaveSettings();

            // 設定を検証
            _settings.Validate();

            // ダイアログを閉じる（OK）
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"設定値が無効です:\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// キャンセルボタンクリック
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
