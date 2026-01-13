using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using H.NotifyIcon;
using Microsoft.Win32;

namespace ComiCap;

/// <summary>
/// App.xaml の相互作用ロジック
/// </summary>
public partial class App : Application
{
    private TaskbarIcon? _trayIcon;
    private MainWindow? _mainWindow;
    private AppSettings _settings;
    private Bitmap? _lastCapturedBitmap;

    /// <summary>
    /// System.Windows.Application.Startup イベントを発生させます。
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 設定を読み込む
        _settings = SettingsManager.Load();

        // アプリケーションを明示的にシャットダウンするまで終了しない
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // システムトレイアイコンを作成
        _trayIcon = (TaskbarIcon)FindResource("TrayIcon");
    }

    /// <summary>
    /// System.Windows.Application.Exit イベントを発生させます。
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        _lastCapturedBitmap?.Dispose();
        _trayIcon?.Dispose();
        base.OnExit(e);
    }

    /// <summary>
    /// メニュー項目 "表示" がクリックされた時の処理
    /// </summary>
    private void MenuItem_Open_Click(object sender, RoutedEventArgs e)
    {
        if (_mainWindow == null)
        {
            _mainWindow = new MainWindow();
            _mainWindow.Closed += (s, args) => _mainWindow = null;
        }

        _mainWindow.Show();
        _mainWindow.Activate();
    }

    /// <summary>
    /// メニュー項目 "キャプチャ" がクリックされた時の処理
    /// </summary>
    private void MenuItem_Capture_Click(object sender, RoutedEventArgs e)
    {
        // MainWindowが存在しない場合は作成
        if (_mainWindow == null)
        {
            MenuItem_Open_Click(sender, e);
        }

        try
        {
            // 前回のBitmapを解放
            _lastCapturedBitmap?.Dispose();

            // スクリーンキャプチャを実行
            _lastCapturedBitmap = CaptureImpl.CaptureScreen();

            // BitmapをWPFのBitmapImageに変換
            using var stream = new MemoryStream();
            _lastCapturedBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            stream.Seek(0, SeekOrigin.Begin);

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = stream;
            bitmapImage.EndInit();
            bitmapImage.Freeze(); // UIスレッド外で使用可能にする

            // MainWindowに画像を設定
            if (_mainWindow != null)
            {
                _mainWindow.CapturedImage.Source = bitmapImage;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"スクリーンキャプチャ中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// メニュー項目 "範囲を選択してキャプチャ" がクリックされた時の処理
    /// </summary>
    private void MenuItem_RegionCapture_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var regionSelector = new RegionSelectorWindow(_settings);
            var result = regionSelector.ShowDialog();

            if (result == true && regionSelector.CapturedBitmap != null)
            {
                // MainWindowが存在しない場合は作成
                if (_mainWindow == null)
                {
                    MenuItem_Open_Click(sender, e);
                }

                // 前回のBitmapを解放
                _lastCapturedBitmap?.Dispose();
                _lastCapturedBitmap = regionSelector.CapturedBitmap;

                // BitmapをWPFのBitmapImageに変換
                using var stream = new MemoryStream();
                _lastCapturedBitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                stream.Seek(0, SeekOrigin.Begin);

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // UIスレッド外で使用可能にする

                // MainWindowに画像を設定
                if (_mainWindow != null)
                {
                    _mainWindow.CapturedImage.Source = bitmapImage;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"領域選択キャプチャ中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// メニュー項目 "名前を付けて保存" がクリックされた時の処理
    /// </summary>
    private void MenuItem_SaveAs_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_lastCapturedBitmap == null)
            {
                MessageBox.Show(
                    "保存する画像がありません。\n先にキャプチャを実行してください。",
                    "情報",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var saveDialog = new SaveFileDialog
            {
                Title = "画像を保存",
                FileName = ImageSaver.GenerateFileName(_settings.GetFileExtension()),
                InitialDirectory = _settings.SaveDirectory,
                Filter = "PNG画像 (*.png)|*.png|JPEG画像 (*.jpg;*.jpeg)|*.jpg;*.jpeg|BMP画像 (*.bmp)|*.bmp|すべてのファイル (*.*)|*.*",
                FilterIndex = _settings.ImageFormat switch
                {
                    ImageFormat.PNG => 1,
                    ImageFormat.JPEG => 2,
                    ImageFormat.BMP => 3,
                    _ => 1
                }
            };

            if (saveDialog.ShowDialog() == true)
            {
                var extension = Path.GetExtension(saveDialog.FileName);
                var format = AppSettings.GetImageFormatFromExtension(extension);

                ImageSaver.Save(_lastCapturedBitmap, saveDialog.FileName, format, _settings.JpegQuality);

                MessageBox.Show(
                    $"画像を保存しました:\n{saveDialog.FileName}",
                    "保存完了",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"画像の保存に失敗しました:\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// メニュー項目 "設定" がクリックされた時の処理
    /// </summary>
    private void MenuItem_Settings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var settingsWindow = new SettingsWindow(_settings);
            if (settingsWindow.ShowDialog() == true)
            {
                // 設定が更新された（OKボタンが押された）
                // 設定は既にSettingsWindowで保存されている
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"設定画面の表示中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// メニュー項目 "終了" がクリックされた時の処理
    /// </summary>
    private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
    {
        Shutdown();
    }
}
