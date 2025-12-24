using System.Globalization;
using WhereIsMyCursor.Resources;

namespace WhereIsMyCursor.Services;

/// <summary>
/// 多語系服務
/// 管理應用程式語言切換
/// </summary>
public static class LocalizationService
{
    /// <summary>
    /// 支援的語言清單
    /// </summary>
    public static readonly (string Code, string Name)[] SupportedLanguages =
    [
        ("zh-TW", "繁體中文"),
        ("en", "English")
    ];

    /// <summary>
    /// 目前語言代碼
    /// </summary>
    public static string CurrentLanguage { get; private set; } = "zh-TW";

    /// <summary>
    /// 設定語言
    /// </summary>
    /// <param name="languageCode">語言代碼 (zh-TW, en)</param>
    public static void SetLanguage(string languageCode)
    {
        CurrentLanguage = languageCode;
        var culture = new CultureInfo(languageCode);
        CultureInfo.CurrentUICulture = culture;
        Strings.Culture = culture;
    }

    /// <summary>
    /// 取得字串資源
    /// </summary>
    public static string GetString(string name)
    {
        return Strings.ResourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;
    }
}
