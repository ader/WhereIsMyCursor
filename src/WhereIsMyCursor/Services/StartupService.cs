using Microsoft.Win32;

namespace WhereIsMyCursor.Services;

/// <summary>
/// 開機自動啟動服務
/// 管理 Windows Registry 中的自動啟動設定
/// </summary>
public static class StartupService
{
    private const string AppName = "WhereIsMyCursor";
    private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    /// 設定開機自動啟動
    /// </summary>
    /// <param name="enable">是否啟用</param>
    public static void SetAutoStart(bool enable)
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: true);
            if (key == null) return;

            if (enable)
            {
                // 取得目前執行檔路徑
                var exePath = Environment.ProcessPath;
                if (!string.IsNullOrEmpty(exePath))
                {
                    key.SetValue(AppName, $"\"{exePath}\"");
                }
            }
            else
            {
                // 移除自動啟動
                key.DeleteValue(AppName, throwOnMissingValue: false);
            }
        }
        catch
        {
            // 忽略 Registry 存取錯誤
        }
    }

    /// <summary>
    /// 檢查是否已設定開機自動啟動
    /// </summary>
    /// <returns>是否已啟用</returns>
    public static bool IsAutoStartEnabled()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, writable: false);
            if (key == null) return false;

            var value = key.GetValue(AppName);
            return value != null;
        }
        catch
        {
            return false;
        }
    }
}
