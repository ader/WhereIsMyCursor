using System.Windows;
using System.Windows.Input;
using WhereIsMyCursor.Services;

namespace WhereIsMyCursor;

/// <summary>
/// 隱藏主視窗
/// 作為全域熱鍵的宿主視窗
/// </summary>
public partial class MainWindow : Window
{
    private HotkeyService? _hotkeyService;

    /// <summary>
    /// 熱鍵觸發事件
    /// </summary>
    public event Action? HotkeyTriggered;

    public MainWindow()
    {
        InitializeComponent();

        // 確保視窗已載入後再註冊熱鍵
        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    /// <summary>
    /// 視窗載入完成
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // 註冊全域熱鍵 (Ctrl + Alt + F)
        _hotkeyService = new HotkeyService();
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;

        try
        {
            _hotkeyService.Register(this, ModifierKeys.Control | ModifierKeys.Alt, Key.F);
        }
        catch (InvalidOperationException ex)
        {
            // 熱鍵註冊失敗，顯示警告
            System.Windows.MessageBox.Show(
                $"無法註冊全域熱鍵 Ctrl+Alt+F：\n{ex.Message}\n\n熱鍵可能已被其他程式佔用。\n您仍可透過系統托盤操作此程式。",
                "熱鍵註冊失敗",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    /// <summary>
    /// 熱鍵按下事件處理
    /// </summary>
    private void OnHotkeyPressed()
    {
        HotkeyTriggered?.Invoke();
    }

    /// <summary>
    /// 視窗關閉
    /// </summary>
    private void OnClosed(object? sender, EventArgs e)
    {
        // 釋放熱鍵服務
        _hotkeyService?.Dispose();
        _hotkeyService = null;
    }
}
