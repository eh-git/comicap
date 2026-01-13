using System;
using System.IO;

namespace ComiCap;

/// <summary>
/// ページめくりキーの種類
/// </summary>
public enum PageTurnKey
{
    Right,      // →キー
    Left,       // ←キー
    Down,       // ↓キー
    Up,         // ↑キー
    Space,      // スペースキー
    PageDown,   // PageDownキー
    PageUp,     // PageUpキー
    Enter       // Enterキー
}

/// <summary>
/// スライド自動キャプチャの設定
/// </summary>
public class SlideCaptureSettings
{
    /// <summary>
    /// ブラウザURL（省略可能）
    /// </summary>
    public string Url { get; set; } = "";

    /// <summary>
    /// ページめくりキー
    /// </summary>
    public PageTurnKey PageTurnKey { get; set; } = PageTurnKey.Right;

    /// <summary>
    /// キャプチャ間隔（ミリ秒）
    /// </summary>
    public int CaptureIntervalMs { get; set; } = 1000;

    /// <summary>
    /// キャプチャ回数（0で無限ループ）
    /// </summary>
    public int CaptureCount { get; set; } = 10;

    /// <summary>
    /// 保存先フォルダ
    /// </summary>
    public string SaveDirectory { get; set; } = "";

    /// <summary>
    /// 画像形式
    /// </summary>
    public ImageFormat ImageFormat { get; set; } = ImageFormat.PNG;

    /// <summary>
    /// JPEG品質（0-100）
    /// </summary>
    public int JpegQuality { get; set; } = 90;

    /// <summary>
    /// ファイル名プレフィックス
    /// </summary>
    public string FileNamePrefix { get; set; } = "slide";

    /// <summary>
    /// ブラウザパス（省略可能）
    /// </summary>
    public string BrowserPath { get; set; } = "";

    /// <summary>
    /// ブラウザを最大化するか
    /// </summary>
    public bool MaximizeBrowser { get; set; } = true;

    /// <summary>
    /// コンストラクタ（デフォルト値で初期化）
    /// </summary>
    public SlideCaptureSettings()
    {
        // デフォルトの保存先をマイピクチャに設定
        SaveDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
    }

    /// <summary>
    /// 既存の設定から初期化するコンストラクタ
    /// </summary>
    public SlideCaptureSettings(AppSettings appSettings)
    {
        SaveDirectory = appSettings.SaveDirectory;
        ImageFormat = appSettings.ImageFormat;
        JpegQuality = appSettings.JpegQuality;
    }

    /// <summary>
    /// 設定を検証
    /// </summary>
    /// <exception cref="ArgumentException">設定値が無効な場合</exception>
    public void Validate()
    {
        if (CaptureIntervalMs < 100 || CaptureIntervalMs > 10000)
        {
            throw new ArgumentException("キャプチャ間隔は100～10000ミリ秒の範囲で指定してください");
        }

        if (CaptureCount < 0 || CaptureCount > 1000)
        {
            throw new ArgumentException("キャプチャ回数は0～1000の範囲で指定してください");
        }

        if (string.IsNullOrWhiteSpace(SaveDirectory))
        {
            throw new ArgumentException("保存先フォルダを指定してください");
        }

        if (!Directory.Exists(SaveDirectory))
        {
            throw new ArgumentException($"保存先フォルダが存在しません: {SaveDirectory}");
        }

        if (string.IsNullOrWhiteSpace(FileNamePrefix))
        {
            throw new ArgumentException("ファイル名プレフィックスを指定してください");
        }

        // ファイル名に使用できない文字をチェック
        var invalidChars = Path.GetInvalidFileNameChars();
        if (FileNamePrefix.IndexOfAny(invalidChars) >= 0)
        {
            throw new ArgumentException("ファイル名プレフィックスに使用できない文字が含まれています");
        }

        if (JpegQuality < 0 || JpegQuality > 100)
        {
            throw new ArgumentException("JPEG品質は0～100の範囲で指定してください");
        }

        if (!string.IsNullOrWhiteSpace(BrowserPath) && !File.Exists(BrowserPath))
        {
            throw new ArgumentException($"ブラウザの実行ファイルが見つかりません: {BrowserPath}");
        }
    }

    /// <summary>
    /// ファイル拡張子を取得
    /// </summary>
    public string GetFileExtension()
    {
        return ImageFormat switch
        {
            ImageFormat.PNG => ".png",
            ImageFormat.JPEG => ".jpg",
            ImageFormat.BMP => ".bmp",
            _ => ".png"
        };
    }

    /// <summary>
    /// 指定された連番のファイル名を生成
    /// </summary>
    /// <param name="index">連番（1から開始）</param>
    /// <returns>ファイル名（拡張子含む）</returns>
    public string GenerateFileName(int index)
    {
        var extension = GetFileExtension();
        return $"{FileNamePrefix}_{index:000}{extension}";
    }

    /// <summary>
    /// フルパスを生成
    /// </summary>
    /// <param name="index">連番（1から開始）</param>
    /// <returns>フルパス</returns>
    public string GenerateFilePath(int index)
    {
        var fileName = GenerateFileName(index);
        return Path.Combine(SaveDirectory, fileName);
    }
}
