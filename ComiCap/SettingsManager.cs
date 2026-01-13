using System;
using System.IO;
using System.Text.Json;

namespace ComiCap;

/// <summary>
/// アプリケーション設定の読み書きを管理するクラス
/// </summary>
public static class SettingsManager
{
    private static readonly string SettingsDirectory =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "ComiCap");

    private static readonly string SettingsFilePath =
        Path.Combine(SettingsDirectory, "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// 設定を読み込む
    /// </summary>
    /// <returns>読み込まれた設定。失敗時はデフォルト設定</returns>
    public static AppSettings Load()
    {
        try
        {
            if (!File.Exists(SettingsFilePath))
            {
                // 設定ファイルが存在しない場合はデフォルト設定を返す
                Console.WriteLine($"設定ファイルが見つかりません。デフォルト設定を使用します: {SettingsFilePath}");
                return new AppSettings();
            }

            var json = File.ReadAllText(SettingsFilePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);

            if (settings == null)
            {
                Console.WriteLine("設定のデシリアライズに失敗しました。デフォルト設定を使用します。");
                return new AppSettings();
            }

            // 設定を検証
            settings.Validate();

            Console.WriteLine($"設定を読み込みました: {SettingsFilePath}");
            return settings;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"設定ファイルのパースに失敗しました: {ex.Message}。デフォルト設定を使用します。");
            return new AppSettings();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"設定の読み込みに失敗しました: {ex.Message}。デフォルト設定を使用します。");
            return new AppSettings();
        }
    }

    /// <summary>
    /// 設定を保存する
    /// </summary>
    /// <param name="settings">保存する設定</param>
    /// <exception cref="IOException">ファイルの書き込みに失敗した場合</exception>
    public static void Save(AppSettings settings)
    {
        try
        {
            // 設定を検証
            settings.Validate();

            // ディレクトリが存在しない場合は作成
            if (!Directory.Exists(SettingsDirectory))
            {
                Directory.CreateDirectory(SettingsDirectory);
                Console.WriteLine($"設定ディレクトリを作成しました: {SettingsDirectory}");
            }

            // JSONにシリアライズして保存
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsFilePath, json);

            Console.WriteLine($"設定を保存しました: {SettingsFilePath}");
        }
        catch (UnauthorizedAccessException ex)
        {
            throw new IOException($"設定ファイルへの書き込み権限がありません: {SettingsFilePath}", ex);
        }
        catch (Exception ex)
        {
            throw new IOException($"設定の保存に失敗しました: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 設定をデフォルト値にリセットする
    /// </summary>
    public static void Reset()
    {
        var defaultSettings = new AppSettings();
        Save(defaultSettings);
        Console.WriteLine("設定をデフォルト値にリセットしました。");
    }

    /// <summary>
    /// 設定ファイルのパスを取得する
    /// </summary>
    /// <returns>設定ファイルの絶対パス</returns>
    public static string GetSettingsFilePath()
    {
        return SettingsFilePath;
    }
}
