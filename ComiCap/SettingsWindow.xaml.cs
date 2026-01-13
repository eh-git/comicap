using System.Windows;
using Microsoft.Win32;
using System.IO;

namespace ComiCap;

/// <summary>
/// SettingsWindow.xaml の相互作用ロジック
/// </summary>
public partial class SettingsWindow : Window
{
    private AppSettings _settings;
    private AppSettings _originalSettings;

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();

        // 設定のコピーを作成（キャンセル時に元に戻せるように）
        _originalSettings = settings;
        _settings = new AppSettings
        {
            SaveDirectory = settings.SaveDirectory,
            ImageFormat = settings.ImageFormat,
            JpegQuality = settings.JpegQuality,
            Version = settings.Version
        };

        LoadSettings();
    }

    /// <summary>
    /// 設定値をUIに読み込む
    /// </summary>
    private void LoadSettings()
    {
        // 保存先
        SaveDirectoryTextBox.Text = _settings.SaveDirectory;

        // 画像形式
        ImageFormatComboBox.Items.Clear();
        ImageFormatComboBox.Items.Add("PNG");
        ImageFormatComboBox.Items.Add("JPEG");
        ImageFormatComboBox.Items.Add("BMP");

        ImageFormatComboBox.SelectedIndex = _settings.ImageFormat switch
        {
            ImageFormat.PNG => 0,
            ImageFormat.JPEG => 1,
            ImageFormat.BMP => 2,
            _ => 0
        };

        // JPEG品質
        JpegQualitySlider.Value = _settings.JpegQuality;
        JpegQualityLabel.Content = _settings.JpegQuality.ToString();

        // JPEG形式以外の場合は品質設定を無効化
        UpdateJpegQualityVisibility();
    }

    /// <summary>
    /// JPEG品質設定の表示/非表示を更新
    /// </summary>
    private void UpdateJpegQualityVisibility()
    {
        var isJpeg = _settings.ImageFormat == ImageFormat.JPEG;
        JpegQualityGroupBox.IsEnabled = isJpeg;
    }

    /// <summary>
    /// 「参照...」ボタンクリック
    /// </summary>
    private void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // FolderBrowserDialogの代わりにOpenFileDialogを使用（WPFではこちらが標準）
            var dialog = new SaveFileDialog
            {
                Title = "保存先フォルダを選択",
                FileName = "フォルダ選択",
                Filter = "フォルダ|dummy",
                CheckFileExists = false,
                CheckPathExists = true,
                InitialDirectory = _settings.SaveDirectory
            };

            // ダイアログ表示前に、簡易的なフォルダ選択方法として
            // Windowsフォルダブラウザダイアログを使用
            var folderDialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = "画像の保存先フォルダを選択してください",
                SelectedPath = _settings.SaveDirectory,
                ShowNewFolderButton = true
            };

            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                _settings.SaveDirectory = folderDialog.SelectedPath;
                SaveDirectoryTextBox.Text = _settings.SaveDirectory;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"フォルダの選択中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 画像形式変更イベント
    /// </summary>
    private void ImageFormatComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ImageFormatComboBox.SelectedIndex < 0)
            return;

        _settings.ImageFormat = ImageFormatComboBox.SelectedIndex switch
        {
            0 => ImageFormat.PNG,
            1 => ImageFormat.JPEG,
            2 => ImageFormat.BMP,
            _ => ImageFormat.PNG
        };

        UpdateJpegQualityVisibility();
    }

    /// <summary>
    /// JPEG品質スライダー変更イベント
    /// </summary>
    private void JpegQualitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (JpegQualityLabel == null)
            return;

        var value = (int)e.NewValue;
        _settings.JpegQuality = value;
        JpegQualityLabel.Content = value.ToString();
    }

    /// <summary>
    /// 「デフォルトに戻す」ボタンクリック
    /// </summary>
    private void ResetButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = MessageBox.Show(
                "すべての設定をデフォルト値に戻しますか？",
                "確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _settings = new AppSettings();
                LoadSettings();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"設定のリセット中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 「OK」ボタンクリック
    /// </summary>
    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // 設定を検証
            _settings.Validate();

            // 元の設定オブジェクトに値をコピー
            _originalSettings.SaveDirectory = _settings.SaveDirectory;
            _originalSettings.ImageFormat = _settings.ImageFormat;
            _originalSettings.JpegQuality = _settings.JpegQuality;
            _originalSettings.Version = _settings.Version;

            // 設定を保存
            SettingsManager.Save(_originalSettings);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"設定の保存に失敗しました:\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 「キャンセル」ボタンクリック
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
