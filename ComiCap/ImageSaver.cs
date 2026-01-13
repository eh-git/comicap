using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ComiCap;

/// <summary>
/// 画像の保存機能を提供する静的クラス
/// </summary>
public static class ImageSaver
{
    /// <summary>
    /// 画像を指定されたパスに保存する
    /// </summary>
    /// <param name="bitmap">保存する画像</param>
    /// <param name="filePath">保存先のファイルパス</param>
    /// <param name="format">画像形式</param>
    /// <param name="jpegQuality">JPEG品質 (0-100、JPEG形式の場合のみ使用)</param>
    /// <exception cref="ArgumentNullException">bitmapまたはfilePathがnullの場合</exception>
    /// <exception cref="IOException">ファイルの書き込みに失敗した場合</exception>
    public static void Save(Bitmap bitmap, string filePath, ImageFormat format, int jpegQuality = 90)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));

        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentNullException(nameof(filePath));

        try
        {
            // 保存先ディレクトリが存在しない場合は作成
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 形式に応じて保存
            switch (format)
            {
                case ImageFormat.PNG:
                    bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
                    break;

                case ImageFormat.JPEG:
                    SaveAsJpeg(bitmap, filePath, jpegQuality);
                    break;

                case ImageFormat.BMP:
                    bitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Bmp);
                    break;

                default:
                    throw new ArgumentException($"サポートされていない画像形式です: {format}", nameof(format));
            }
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            throw new IOException($"画像の保存に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// JPEG形式で画像を保存する（品質指定付き）
    /// </summary>
    private static void SaveAsJpeg(Bitmap bitmap, string filePath, int quality)
    {
        // 品質を0-100の範囲に制限
        quality = Math.Clamp(quality, 0, 100);

        using var encoderParams = new EncoderParameters(1);
        encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, (long)quality);

        var jpegEncoder = GetEncoderInfo("image/jpeg");
        if (jpegEncoder == null)
        {
            throw new IOException("JPEGエンコーダーが見つかりません");
        }

        bitmap.Save(filePath, jpegEncoder, encoderParams);
    }

    /// <summary>
    /// 指定されたMIMEタイプのImageCodecInfoを取得する
    /// </summary>
    private static ImageCodecInfo? GetEncoderInfo(string mimeType)
    {
        var encoders = ImageCodecInfo.GetImageEncoders();
        foreach (var encoder in encoders)
        {
            if (encoder.MimeType == mimeType)
                return encoder;
        }
        return null;
    }

    /// <summary>
    /// タイムスタンプ付きのファイル名を生成する
    /// </summary>
    /// <param name="extension">拡張子（ドット付き）</param>
    /// <returns>生成されたファイル名</returns>
    public static string GenerateFileName(string extension)
    {
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return $"Screenshot_{timestamp}{extension}";
    }

    /// <summary>
    /// ファイル名の重複を回避する
    /// </summary>
    /// <param name="directory">保存先ディレクトリ</param>
    /// <param name="fileName">ファイル名</param>
    /// <returns>重複しないファイル名</returns>
    public static string GetUniqueFileName(string directory, string fileName)
    {
        var fullPath = Path.Combine(directory, fileName);

        if (!File.Exists(fullPath))
        {
            return fileName;
        }

        // ファイルが存在する場合は連番を追加
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName);
        var counter = 1;

        while (true)
        {
            var newFileName = $"{fileNameWithoutExt}_{counter}{extension}";
            fullPath = Path.Combine(directory, newFileName);

            if (!File.Exists(fullPath))
            {
                return newFileName;
            }

            counter++;

            // 無限ループ防止
            if (counter > 9999)
            {
                throw new IOException("一意なファイル名を生成できませんでした");
            }
        }
    }

    /// <summary>
    /// デフォルト設定でキャプチャ画像を保存する
    /// </summary>
    /// <param name="bitmap">保存する画像</param>
    /// <param name="settings">アプリケーション設定</param>
    /// <returns>保存されたファイルのフルパス</returns>
    /// <exception cref="ArgumentNullException">引数がnullの場合</exception>
    /// <exception cref="IOException">保存に失敗した場合</exception>
    public static string SaveWithDefaultSettings(Bitmap bitmap, AppSettings settings)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));

        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        // ファイル名と拡張子を生成
        var extension = settings.GetFileExtension();
        var fileName = GenerateFileName(extension);

        // 重複しないファイル名を取得
        fileName = GetUniqueFileName(settings.SaveDirectory, fileName);

        // フルパスを生成
        var fullPath = Path.Combine(settings.SaveDirectory, fileName);

        // 保存
        Save(bitmap, fullPath, settings.ImageFormat, settings.JpegQuality);

        return fullPath;
    }
}
