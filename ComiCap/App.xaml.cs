using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Drawing.Imaging;
using H.NotifyIcon;

namespace ComiCap;

/// <summary>
/// App.xaml の相互作用ロジック
/// </summary>
public partial class App : Application
{
    private TaskbarIcon? _trayIcon;
    private MainWindow? _mainWindow;

    /// <summary>
    /// System.Windows.Application.Startup イベントを発生させます。
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

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
            // スクリーンキャプチャを実行
            using var bitmap = CaptureImpl.CaptureScreen();

            // BitmapをWPFのBitmapImageに変換
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
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
    /// メニュー項目 "終了" がクリックされた時の処理
    /// </summary>
    private void MenuItem_Exit_Click(object sender, RoutedEventArgs e)
    {
        Shutdown();
    }
}
