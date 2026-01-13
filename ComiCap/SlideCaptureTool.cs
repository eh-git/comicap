using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ComiCap;

/// <summary>
/// スライド自動キャプチャツール
/// </summary>
public static partial class SlideCaptureTool
{
    // Windows API定義
    [LibraryImport("user32.dll")]
    private static partial IntPtr GetForegroundWindow();

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool SetForegroundWindow(IntPtr hWnd);

    [LibraryImport("user32.dll")]
    private static partial uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray)] INPUT[] pInputs, int cbSize);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_MAXIMIZE = 3;

    // 入力構造体
    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public uint type;
        public InputUnion u;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;
        [FieldOffset(0)]
        public KEYBDINPUT ki;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private const uint INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;

    // 仮想キーコード
    private static ushort GetVirtualKeyCode(PageTurnKey key)
    {
        return key switch
        {
            PageTurnKey.Right => 0x27,      // VK_RIGHT
            PageTurnKey.Left => 0x25,       // VK_LEFT
            PageTurnKey.Down => 0x28,       // VK_DOWN
            PageTurnKey.Up => 0x26,         // VK_UP
            PageTurnKey.Space => 0x20,      // VK_SPACE
            PageTurnKey.PageDown => 0x22,   // VK_NEXT
            PageTurnKey.PageUp => 0x21,     // VK_PRIOR
            PageTurnKey.Enter => 0x0D,      // VK_RETURN
            _ => 0x27
        };
    }

    /// <summary>
    /// スライドキャプチャを実行
    /// </summary>
    /// <param name="settings">キャプチャ設定</param>
    /// <param name="progress">進捗報告用のProgress</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>キャプチャ枚数</returns>
    public static async Task<int> ExecuteCaptureAsync(
        SlideCaptureSettings settings,
        IProgress<SlideCaptureProgress>? progress,
        CancellationToken cancellationToken)
    {
        Process? browserProcess = null;
        var capturedCount = 0;

        try
        {
            // ブラウザ起動
            if (!string.IsNullOrWhiteSpace(settings.Url))
            {
                progress?.Report(new SlideCaptureProgress { Message = "ブラウザを起動中..." });
                browserProcess = await StartBrowserAsync(settings);

                // ブラウザ起動後の待機
                await Task.Delay(2000, cancellationToken);

                // ブラウザウィンドウを最大化
                if (settings.MaximizeBrowser && browserProcess?.MainWindowHandle != IntPtr.Zero)
                {
                    ShowWindow(browserProcess.MainWindowHandle, SW_MAXIMIZE);
                    await Task.Delay(500, cancellationToken);
                }
            }

            // キャプチャ対象ウィンドウを取得
            var targetWindow = GetForegroundWindow();
            if (targetWindow == IntPtr.Zero)
            {
                throw new InvalidOperationException("キャプチャ対象のウィンドウが見つかりません");
            }

            // キャプチャループ
            var maxCount = settings.CaptureCount == 0 ? int.MaxValue : settings.CaptureCount;
            for (var i = 1; i <= maxCount; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 進捗報告
                progress?.Report(new SlideCaptureProgress
                {
                    Current = i,
                    Total = settings.CaptureCount,
                    Message = settings.CaptureCount == 0
                        ? $"キャプチャ中: {i}枚目"
                        : $"キャプチャ中: {i}/{settings.CaptureCount}"
                });

                // ウィンドウをフォアグラウンドに
                SetForegroundWindow(targetWindow);
                await Task.Delay(100, cancellationToken);

                // キャプチャ実行
                using (var bitmap = CaptureImpl.CaptureActiveWindow())
                {
                    var filePath = settings.GenerateFilePath(i);
                    ImageSaver.Save(bitmap, filePath, settings.ImageFormat, settings.JpegQuality);
                    capturedCount++;
                }

                // 最後のページでなければページめくり
                if (i < maxCount)
                {
                    // キャプチャ間隔待機
                    await Task.Delay(settings.CaptureIntervalMs, cancellationToken);

                    // ページめくりキー送信
                    SendPageTurnKey(settings.PageTurnKey);

                    // ページ遷移待機
                    await Task.Delay(settings.CaptureIntervalMs, cancellationToken);
                }
            }

            return capturedCount;
        }
        finally
        {
            // ブラウザプロセスのクリーンアップ（必要に応じて）
            // 注: ユーザーが継続して使用する可能性があるため、プロセスは終了させない
        }
    }

    /// <summary>
    /// ブラウザを起動
    /// </summary>
    private static async Task<Process> StartBrowserAsync(SlideCaptureSettings settings)
    {
        var startInfo = new ProcessStartInfo();

        if (!string.IsNullOrWhiteSpace(settings.BrowserPath))
        {
            // ブラウザパスが指定されている場合
            startInfo.FileName = settings.BrowserPath;
            startInfo.Arguments = settings.Url;
        }
        else
        {
            // デフォルトブラウザを使用
            startInfo.FileName = settings.Url;
            startInfo.UseShellExecute = true;
        }

        var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new InvalidOperationException("ブラウザの起動に失敗しました");
        }

        // プロセスがウィンドウを作成するまで待機
        await Task.Run(() =>
        {
            process.WaitForInputIdle(5000);
        });

        return process;
    }

    /// <summary>
    /// ページめくりキーを送信
    /// </summary>
    private static void SendPageTurnKey(PageTurnKey key)
    {
        var vkCode = GetVirtualKeyCode(key);

        var inputs = new INPUT[2];

        // キーダウン
        inputs[0] = new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = vkCode,
                    wScan = 0,
                    dwFlags = 0,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        // キーアップ
        inputs[1] = new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion
            {
                ki = new KEYBDINPUT
                {
                    wVk = vkCode,
                    wScan = 0,
                    dwFlags = KEYEVENTF_KEYUP,
                    time = 0,
                    dwExtraInfo = IntPtr.Zero
                }
            }
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
    }
}

/// <summary>
/// スライドキャプチャの進捗情報
/// </summary>
public class SlideCaptureProgress
{
    /// <summary>
    /// 現在のキャプチャ番号
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// 合計キャプチャ数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// メッセージ
    /// </summary>
    public string Message { get; set; } = "";
}
