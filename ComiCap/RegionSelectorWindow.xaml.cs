using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.Win32;
using System.IO;

namespace ComiCap;

/// <summary>
/// RegionSelectorWindow.xaml の相互作用ロジック
/// 画面領域選択用の透過ウィンドウ
/// </summary>
public partial class RegionSelectorWindow : Window
{
    private enum DragMode
    {
        None,
        Creating,
        Moving,
        Resizing
    }

    private enum ResizeHandle
    {
        None,
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    private DragMode _dragMode = DragMode.None;
    private ResizeHandle _resizeHandle = ResizeHandle.None;
    private System.Windows.Point _dragStartPoint;
    private Rect _selectionRect;
    private Bitmap? _capturedBitmap;
    private readonly AppSettings _settings;

    /// <summary>
    /// キャプチャされた画像を取得
    /// </summary>
    public Bitmap? CapturedBitmap => _capturedBitmap;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public RegionSelectorWindow(AppSettings settings)
    {
        InitializeComponent();
        _settings = settings;

        // ウィンドウサイズを画面全体に設定
        Left = 0;
        Top = 0;
        Width = SystemParameters.PrimaryScreenWidth;
        Height = SystemParameters.PrimaryScreenHeight;
    }

    /// <summary>
    /// ウィンドウのマウスダウンイベント
    /// </summary>
    private void Window_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
            return;

        _dragStartPoint = e.GetPosition(this);

        // 新規選択開始
        _dragMode = DragMode.Creating;
        _selectionRect = new Rect(_dragStartPoint, _dragStartPoint);
        UpdateSelectionDisplay();
    }

    /// <summary>
    /// ハンドルのマウスダウンイベント
    /// </summary>
    private void Handle_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
            return;

        e.Handled = true;
        _dragStartPoint = e.GetPosition(this);
        _dragMode = DragMode.Resizing;

        // どのハンドルがクリックされたか判定
        if (sender == HandleTopLeft) _resizeHandle = ResizeHandle.TopLeft;
        else if (sender == HandleTopCenter) _resizeHandle = ResizeHandle.TopCenter;
        else if (sender == HandleTopRight) _resizeHandle = ResizeHandle.TopRight;
        else if (sender == HandleMiddleLeft) _resizeHandle = ResizeHandle.MiddleLeft;
        else if (sender == HandleMiddleRight) _resizeHandle = ResizeHandle.MiddleRight;
        else if (sender == HandleBottomLeft) _resizeHandle = ResizeHandle.BottomLeft;
        else if (sender == HandleBottomCenter) _resizeHandle = ResizeHandle.BottomCenter;
        else if (sender == HandleBottomRight) _resizeHandle = ResizeHandle.BottomRight;

        Mouse.Capture((UIElement)sender);
    }

    /// <summary>
    /// ウィンドウのマウス移動イベント
    /// </summary>
    private void Window_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed)
            return;

        var currentPoint = e.GetPosition(this);

        switch (_dragMode)
        {
            case DragMode.Creating:
                // 選択範囲を作成中
                _selectionRect = new Rect(_dragStartPoint, currentPoint);
                UpdateSelectionDisplay();
                break;

            case DragMode.Moving:
                // 選択範囲を移動中
                var delta = currentPoint - _dragStartPoint;
                _selectionRect.Offset(delta.X, delta.Y);
                _dragStartPoint = currentPoint;
                UpdateSelectionDisplay();
                break;

            case DragMode.Resizing:
                // 選択範囲をリサイズ中
                ResizeSelection(currentPoint);
                UpdateSelectionDisplay();
                break;
        }
    }

    /// <summary>
    /// ウィンドウのマウスアップイベント
    /// </summary>
    private void Window_MouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
            return;

        if (_dragMode == DragMode.Creating)
        {
            // 選択範囲が作成された
            if (_selectionRect.Width > 10 && _selectionRect.Height > 10)
            {
                // 有効な選択範囲
                RegionCaptureButton.IsEnabled = true;
                SaveButton.IsEnabled = true;
                Cursor = Cursors.Arrow;
            }
        }

        _dragMode = DragMode.None;
        _resizeHandle = ResizeHandle.None;
        Mouse.Capture(null);
    }

    /// <summary>
    /// 選択範囲をリサイズ
    /// </summary>
    private void ResizeSelection(System.Windows.Point currentPoint)
    {
        var rect = _selectionRect;

        switch (_resizeHandle)
        {
            case ResizeHandle.TopLeft:
                rect = new Rect(currentPoint, rect.BottomRight);
                break;
            case ResizeHandle.TopCenter:
                rect = new Rect(new System.Windows.Point(rect.Left, currentPoint.Y), rect.BottomRight);
                break;
            case ResizeHandle.TopRight:
                rect = new Rect(new System.Windows.Point(rect.Left, currentPoint.Y), new System.Windows.Point(currentPoint.X, rect.Bottom));
                break;
            case ResizeHandle.MiddleLeft:
                rect = new Rect(new System.Windows.Point(currentPoint.X, rect.Top), rect.BottomRight);
                break;
            case ResizeHandle.MiddleRight:
                rect = new Rect(rect.TopLeft, new System.Windows.Point(currentPoint.X, rect.Bottom));
                break;
            case ResizeHandle.BottomLeft:
                rect = new Rect(new System.Windows.Point(currentPoint.X, rect.Top), new System.Windows.Point(rect.Right, currentPoint.Y));
                break;
            case ResizeHandle.BottomCenter:
                rect = new Rect(rect.TopLeft, new System.Windows.Point(rect.Right, currentPoint.Y));
                break;
            case ResizeHandle.BottomRight:
                rect = new Rect(rect.TopLeft, currentPoint);
                break;
        }

        _selectionRect = rect;
    }

    /// <summary>
    /// 選択範囲の表示を更新
    /// </summary>
    private void UpdateSelectionDisplay()
    {
        var rect = _selectionRect;

        // 負の幅・高さを修正
        if (rect.Width < 0)
        {
            rect.X += rect.Width;
            rect.Width = -rect.Width;
        }
        if (rect.Height < 0)
        {
            rect.Y += rect.Height;
            rect.Height = -rect.Height;
        }

        _selectionRect = rect;

        // 選択矩形を更新
        Canvas.SetLeft(SelectionRectangle, rect.X);
        Canvas.SetTop(SelectionRectangle, rect.Y);
        SelectionRectangle.Width = rect.Width;
        SelectionRectangle.Height = rect.Height;
        SelectionRectangle.Visibility = Visibility.Visible;

        // オーバーレイマスクを更新（選択範囲を透明に）
        UpdateOverlayMask();

        // リサイズハンドルを更新
        UpdateHandles();
    }

    /// <summary>
    /// オーバーレイマスクを更新（選択範囲に穴を開ける）
    /// </summary>
    private void UpdateOverlayMask()
    {
        var fullRect = new RectangleGeometry(new Rect(0, 0, Width, Height));
        var selectionGeometry = new RectangleGeometry(_selectionRect);

        var combinedGeometry = new CombinedGeometry(
            GeometryCombineMode.Exclude,
            fullRect,
            selectionGeometry);

        var path = new System.Windows.Shapes.Path
        {
            Fill = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 0, 0, 0)),
            Data = combinedGeometry
        };

        OverlayCanvas.Children.Clear();
        OverlayCanvas.Children.Add(path);
    }

    /// <summary>
    /// リサイズハンドルの位置を更新
    /// </summary>
    private void UpdateHandles()
    {
        if (_selectionRect.Width < 10 || _selectionRect.Height < 10)
        {
            HideHandles();
            return;
        }

        var rect = _selectionRect;
        var handleSize = 10.0;
        var offset = handleSize / 2;

        // 8つのハンドルの位置を設定
        SetHandlePosition(HandleTopLeft, rect.Left - offset, rect.Top - offset);
        SetHandlePosition(HandleTopCenter, rect.Left + rect.Width / 2 - offset, rect.Top - offset);
        SetHandlePosition(HandleTopRight, rect.Right - offset, rect.Top - offset);
        SetHandlePosition(HandleMiddleLeft, rect.Left - offset, rect.Top + rect.Height / 2 - offset);
        SetHandlePosition(HandleMiddleRight, rect.Right - offset, rect.Top + rect.Height / 2 - offset);
        SetHandlePosition(HandleBottomLeft, rect.Left - offset, rect.Bottom - offset);
        SetHandlePosition(HandleBottomCenter, rect.Left + rect.Width / 2 - offset, rect.Bottom - offset);
        SetHandlePosition(HandleBottomRight, rect.Right - offset, rect.Bottom - offset);

        ShowHandles();
    }

    /// <summary>
    /// ハンドルの位置を設定
    /// </summary>
    private void SetHandlePosition(UIElement handle, double x, double y)
    {
        Canvas.SetLeft(handle, x);
        Canvas.SetTop(handle, y);
    }

    /// <summary>
    /// すべてのハンドルを表示
    /// </summary>
    private void ShowHandles()
    {
        HandleTopLeft.Visibility = Visibility.Visible;
        HandleTopCenter.Visibility = Visibility.Visible;
        HandleTopRight.Visibility = Visibility.Visible;
        HandleMiddleLeft.Visibility = Visibility.Visible;
        HandleMiddleRight.Visibility = Visibility.Visible;
        HandleBottomLeft.Visibility = Visibility.Visible;
        HandleBottomCenter.Visibility = Visibility.Visible;
        HandleBottomRight.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// すべてのハンドルを非表示
    /// </summary>
    private void HideHandles()
    {
        HandleTopLeft.Visibility = Visibility.Collapsed;
        HandleTopCenter.Visibility = Visibility.Collapsed;
        HandleTopRight.Visibility = Visibility.Collapsed;
        HandleMiddleLeft.Visibility = Visibility.Collapsed;
        HandleMiddleRight.Visibility = Visibility.Collapsed;
        HandleBottomLeft.Visibility = Visibility.Collapsed;
        HandleBottomCenter.Visibility = Visibility.Collapsed;
        HandleBottomRight.Visibility = Visibility.Collapsed;
    }

    /// <summary>
    /// キーボードイベント処理
    /// </summary>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                // キャンセル
                CancelButton_Click(sender, e);
                break;

            case Key.Enter:
                // 選択範囲キャプチャ
                if (RegionCaptureButton.IsEnabled)
                    RegionCaptureButton_Click(sender, e);
                break;

            case Key.F:
                // 全画面キャプチャ
                FullScreenCaptureButton_Click(sender, e);
                break;

            case Key.S:
                // Ctrl+S で保存
                if (Keyboard.Modifiers == ModifierKeys.Control && SaveButton.IsEnabled)
                    SaveButton_Click(sender, e);
                break;
        }
    }

    /// <summary>
    /// 全画面キャプチャボタンクリック
    /// </summary>
    private void FullScreenCaptureButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // ウィンドウを一時的に非表示
            Hide();
            System.Threading.Thread.Sleep(100); // 画面更新を待つ

            // 全画面キャプチャ
            _capturedBitmap?.Dispose();
            _capturedBitmap = CaptureImpl.CaptureScreen();

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            Show();
            MessageBox.Show(
                $"全画面キャプチャ中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 選択範囲キャプチャボタンクリック
    /// </summary>
    private void RegionCaptureButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_selectionRect.Width < 1 || _selectionRect.Height < 1)
            {
                MessageBox.Show(
                    "選択範囲が小さすぎます。\nもっと大きな範囲を選択してください。",
                    "情報",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // ウィンドウを一時的に非表示
            Hide();
            System.Threading.Thread.Sleep(100); // 画面更新を待つ

            // 選択範囲をキャプチャ
            _capturedBitmap?.Dispose();
            _capturedBitmap = CaptureImpl.CaptureRegion(
                (int)_selectionRect.X,
                (int)_selectionRect.Y,
                (int)_selectionRect.Width,
                (int)_selectionRect.Height);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            Show();
            MessageBox.Show(
                $"選択範囲のキャプチャ中にエラーが発生しました:\n{ex.Message}",
                "エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 保存ボタンクリック
    /// </summary>
    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_selectionRect.Width < 1 || _selectionRect.Height < 1)
            {
                MessageBox.Show(
                    "選択範囲が小さすぎます。\nもっと大きな範囲を選択してください。",
                    "情報",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // ウィンドウを一時的に非表示
            Hide();
            System.Threading.Thread.Sleep(100); // 画面更新を待つ

            // 選択範囲をキャプチャ
            using var bitmap = CaptureImpl.CaptureRegion(
                (int)_selectionRect.X,
                (int)_selectionRect.Y,
                (int)_selectionRect.Width,
                (int)_selectionRect.Height);

            Show();

            // 保存ダイアログを表示
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
                var extension = System.IO.Path.GetExtension(saveDialog.FileName);
                var format = AppSettings.GetImageFormatFromExtension(extension);

                ImageSaver.Save(bitmap, saveDialog.FileName, format, _settings.JpegQuality);

                MessageBox.Show(
                    $"画像を保存しました:\n{saveDialog.FileName}",
                    "保存完了",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                // 保存後はウィンドウを閉じる
                DialogResult = false;
                Close();
            }
        }
        catch (Exception ex)
        {
            Show();
            MessageBox.Show(
                $"画像の保存に失敗しました:\n{ex.Message}",
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

    /// <summary>
    /// ウィンドウクローズ時のクリーンアップ
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        if (DialogResult != true)
        {
            _capturedBitmap?.Dispose();
            _capturedBitmap = null;
        }
        base.OnClosed(e);
    }
}
