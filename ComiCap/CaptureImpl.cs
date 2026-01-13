using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ComiCap;

/// <summary>
/// スクリーンキャプチャ機能を提供する静的クラス
/// </summary>
static partial class CaptureImpl
{
    private const int SRCCOPY = 13369376;
    private const int CAPTUREBLT = 1073741824;

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetDC(IntPtr hwnd);

    [LibraryImport("gdi32.dll")]
    private static partial int BitBlt(
        IntPtr hDestDC,
        int x,
        int y,
        int nWidth,
        int nHeight,
        IntPtr hSrcDC,
        int xSrc,
        int ySrc,
        int dwRop);

    [LibraryImport("user32.dll")]
    private static partial IntPtr ReleaseDC(IntPtr hwnd, IntPtr hdc);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetWindowDC(IntPtr hwnd);

    [LibraryImport("user32.dll")]
    private static partial IntPtr GetForegroundWindow();

    [LibraryImport("user32.dll")]
    private static partial int GetWindowRect(IntPtr hwnd, ref RECT lpRect);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }

    /// <summary>
    /// プライマリスクリーンの画像を取得します
    /// </summary>
    /// <returns>プライマリスクリーンのBitmap画像</returns>
    /// <exception cref="InvalidOperationException">キャプチャに失敗した場合</exception>
    public static Bitmap CaptureScreen()
    {
        IntPtr disDC = IntPtr.Zero;
        IntPtr hDC = IntPtr.Zero;
        Bitmap? bmp = null;
        Graphics? g = null;

        try
        {
            // プライマリモニタのデバイスコンテキストを取得
            disDC = GetDC(IntPtr.Zero);
            if (disDC == IntPtr.Zero)
            {
                throw new InvalidOperationException("デバイスコンテキストの取得に失敗しました");
            }

            // Bitmapの作成
            var screenBounds = Screen.PrimaryScreen?.Bounds
                ?? throw new InvalidOperationException("プライマリスクリーンの情報を取得できません");

            bmp = new Bitmap(screenBounds.Width, screenBounds.Height);

            // Graphicsの作成
            g = Graphics.FromImage(bmp);
            hDC = g.GetHdc();

            // Bitmapに画像をコピー
            var result = BitBlt(hDC, 0, 0, bmp.Width, bmp.Height, disDC, 0, 0, SRCCOPY);
            if (result == 0)
            {
                throw new InvalidOperationException("画面のキャプチャに失敗しました");
            }

            return bmp;
        }
        catch
        {
            bmp?.Dispose();
            throw;
        }
        finally
        {
            // リソースの解放
            if (hDC != IntPtr.Zero && g != null)
            {
                g.ReleaseHdc(hDC);
            }
            g?.Dispose();

            if (disDC != IntPtr.Zero)
            {
                ReleaseDC(IntPtr.Zero, disDC);
            }
        }
    }

    /// <summary>
    /// アクティブなウィンドウの画像を取得します
    /// </summary>
    /// <returns>アクティブなウィンドウのBitmap画像</returns>
    /// <exception cref="InvalidOperationException">キャプチャに失敗した場合</exception>
    public static Bitmap CaptureActiveWindow()
    {
        IntPtr winDC = IntPtr.Zero;
        IntPtr hDC = IntPtr.Zero;
        Bitmap? bmp = null;
        Graphics? g = null;

        try
        {
            // アクティブなウィンドウのハンドルとデバイスコンテキストを取得
            var hWnd = GetForegroundWindow();
            if (hWnd == IntPtr.Zero)
            {
                throw new InvalidOperationException("アクティブなウィンドウが見つかりません");
            }

            winDC = GetWindowDC(hWnd);
            if (winDC == IntPtr.Zero)
            {
                throw new InvalidOperationException("ウィンドウのデバイスコンテキストの取得に失敗しました");
            }

            // ウィンドウの大きさを取得
            var winRect = new RECT();
            if (GetWindowRect(hWnd, ref winRect) == 0)
            {
                throw new InvalidOperationException("ウィンドウのサイズ情報の取得に失敗しました");
            }

            var width = winRect.right - winRect.left;
            var height = winRect.bottom - winRect.top;

            if (width <= 0 || height <= 0)
            {
                throw new InvalidOperationException("ウィンドウのサイズが無効です");
            }

            // Bitmapの作成
            bmp = new Bitmap(width, height);

            // Graphicsの作成
            g = Graphics.FromImage(bmp);
            hDC = g.GetHdc();

            // Bitmapに画像をコピー
            var result = BitBlt(hDC, 0, 0, bmp.Width, bmp.Height, winDC, 0, 0, SRCCOPY);
            if (result == 0)
            {
                throw new InvalidOperationException("ウィンドウのキャプチャに失敗しました");
            }

            return bmp;
        }
        catch
        {
            bmp?.Dispose();
            throw;
        }
        finally
        {
            // リソースの解放
            if (hDC != IntPtr.Zero && g != null)
            {
                g.ReleaseHdc(hDC);
            }
            g?.Dispose();

            if (winDC != IntPtr.Zero)
            {
                ReleaseDC(IntPtr.Zero, winDC);
            }
        }
    }
}
