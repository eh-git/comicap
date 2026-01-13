using System;
using System.IO;

namespace ComiCap;

/// <summary>
/// アプリケーションの設定を管理するクラス
/// </summary>
public class AppSettings
{
    /// <summary>
    /// デフォルトの保存先ディレクトリ
    /// </summary>
    public string SaveDirectory { get; set; }

    /// <summary>
    /// デフォルトの画像形式
    /// </summary>
    public ImageFormat ImageFormat { get; set; }

    /// <summary>
    /// JPEG品質 (0-100)
    /// </summary>
    public int JpegQuality { get; set; }

    /// <summary>
    /// 設定のバージョン
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    /// デフォルトコンストラクタ（デフォルト値で初期化）
    /// </summary>
    public AppSettings()
    {
        SaveDirectory = GetDefaultSaveDirectory();
        ImageFormat = ImageFormat.PNG;
        JpegQuality = 90;
        Version = "2.1.0";
    }

    /// <summary>
    /// デフォルトの保存先ディレクトリを取得
    /// </summary>
    /// <returns>デフォルトの保存先パス</returns>
    public static string GetDefaultSaveDirectory()
    {
        var picturesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        return Path.Combine(picturesPath, "ComiCap");
    }

    /// <summary>
    /// 設定を検証し、無効な値を修正する
    /// </summary>
    public void Validate()
    {
        // JPEG品質を0-100の範囲に制限
        if (JpegQuality < 0) JpegQuality = 0;
        if (JpegQuality > 100) JpegQuality = 100;

        // 保存先ディレクトリが空の場合はデフォルト値を使用
        if (string.IsNullOrWhiteSpace(SaveDirectory))
        {
            SaveDirectory = GetDefaultSaveDirectory();
        }

        // バージョンが空の場合はデフォルト値を使用
        if (string.IsNullOrWhiteSpace(Version))
        {
            Version = "2.1.0";
        }
    }

    /// <summary>
    /// 画像形式に対応する拡張子を取得
    /// </summary>
    /// <returns>拡張子（ドット付き）</returns>
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
    /// 拡張子から画像形式を推測
    /// </summary>
    /// <param name="extension">拡張子（ドット付き）</param>
    /// <returns>画像形式</returns>
    public static ImageFormat GetImageFormatFromExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".png" => ImageFormat.PNG,
            ".jpg" or ".jpeg" => ImageFormat.JPEG,
            ".bmp" => ImageFormat.BMP,
            _ => ImageFormat.PNG
        };
    }
}
