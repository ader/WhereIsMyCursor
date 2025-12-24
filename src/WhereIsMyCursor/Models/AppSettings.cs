using System.IO;
using System.Text.Json;

namespace WhereIsMyCursor.Models;

/// <summary>
/// 更新頻率等級
/// </summary>
public enum UpdateFrequencyLevel
{
    /// <summary>低頻率 (2 FPS)</summary>
    Low,
    /// <summary>中頻率 (10 FPS)</summary>
    Medium,
    /// <summary>高頻率 (30 FPS)</summary>
    High
}

/// <summary>
/// 應用程式設定模型
/// </summary>
public class AppSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WhereIsMyCursor",
        "settings.json"
    );

    /// <summary>
    /// 是否啟用動態托盤圖示
    /// </summary>
    public bool DynamicIconEnabled { get; set; } = true;

    /// <summary>
    /// 動態圖示更新頻率
    /// </summary>
    public UpdateFrequencyLevel UpdateFrequency { get; set; } = UpdateFrequencyLevel.Medium;

    /// <summary>
    /// 托盤圖示位置 X 偏移量 (從螢幕右邊緣往左的像素數)
    /// </summary>
    public int TrayIconOffsetX { get; set; } = 100;

    /// <summary>
    /// 托盤圖示位置 Y 偏移量 (從工作列中心的像素數，正值向下)
    /// </summary>
    public int TrayIconOffsetY { get; set; } = 0;

    /// <summary>
    /// 儲存設定
    /// </summary>
    public void Save()
    {
        try
        {
            // 確保目錄存在
            var directory = Path.GetDirectoryName(SettingsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // 序列化並儲存
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            var json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(SettingsPath, json);
        }
        catch
        {
            // 忽略儲存錯誤
        }
    }

    /// <summary>
    /// 載入設定
    /// </summary>
    /// <returns>設定物件</returns>
    public static AppSettings Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
            {
                var json = File.ReadAllText(SettingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);
                return settings ?? new AppSettings();
            }
        }
        catch
        {
            // 忽略載入錯誤
        }

        return new AppSettings();
    }

    /// <summary>
    /// 複製設定
    /// </summary>
    public AppSettings Clone()
    {
        return new AppSettings
        {
            DynamicIconEnabled = this.DynamicIconEnabled,
            UpdateFrequency = this.UpdateFrequency,
            TrayIconOffsetX = this.TrayIconOffsetX,
            TrayIconOffsetY = this.TrayIconOffsetY
        };
    }
}
