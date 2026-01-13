using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ComiCap;

/// <summary>
/// SlideCaptureProgressWindow.xaml の相互作用ロジック
/// </summary>
public partial class SlideCaptureProgressWindow : Window
{
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly SlideCaptureSettings _settings;
    private Task<int>? _captureTask;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public SlideCaptureProgressWindow(SlideCaptureSettings settings)
    {
        InitializeComponent();
        _settings = settings;
        _cancellationTokenSource = new CancellationTokenSource();

        // ウィンドウ読み込み完了後にキャプチャ開始
        Loaded += Window_Loaded;
    }

    /// <summary>
    /// ウィンドウ読み込み完了イベント
    /// </summary>
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await StartCaptureAsync();
    }

    /// <summary>
    /// キャプチャ開始
    /// </summary>
    private async Task StartCaptureAsync()
    {
        try
        {
            var progress = new Progress<SlideCaptureProgress>(UpdateProgress);

            _captureTask = SlideCaptureTool.ExecuteCaptureAsync(
                _settings,
                progress,
                _cancellationTokenSource.Token);

            var capturedCount = await _captureTask;

            // キャプチャ完了
            MessageBox.Show(
                $"キャプチャが完了しました。\n{capturedCount}枚の画像を保存しました。\n\n保存先: {_settings.SaveDirectory}",
                "完了",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = true;
            Close();
        }
        catch (OperationCanceledException)
        {
            // ユーザーによるキャンセル
            MessageBox.Show(
                "キャプチャが中止されました。",
                "中止",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            DialogResult = false;
            Close();
        }
        catch (Exception ex)
        {
            // エラー発生
            MessageBox.Show(
                $"キャプチャ中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            DialogResult = false;
            Close();
        }
    }

    /// <summary>
    /// 進捗更新
    /// </summary>
    private void UpdateProgress(SlideCaptureProgress progress)
    {
        StatusLabel.Text = progress.Message;

        if (progress.Total > 0)
        {
            ProgressBar.Maximum = progress.Total;
            ProgressBar.Value = progress.Current;
            DetailLabel.Text = $"{progress.Current}/{progress.Total} 完了";
        }
        else
        {
            // 無限ループモード
            ProgressBar.IsIndeterminate = true;
            DetailLabel.Text = $"{progress.Current}枚キャプチャ済み";
        }
    }

    /// <summary>
    /// 停止ボタンクリック
    /// </summary>
    private void StopButton_Click(object sender, RoutedEventArgs e)
    {
        StopButton.IsEnabled = false;
        StatusLabel.Text = "停止中...";
        _cancellationTokenSource.Cancel();
    }

    /// <summary>
    /// ウィンドウクローズ時のクリーンアップ
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        base.OnClosed(e);
    }
}
