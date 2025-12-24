using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using WhereIsMyCursor.Interop;

namespace WhereIsMyCursor.Services;

/// <summary>
/// 全域熱鍵服務
/// 負責註冊與處理全域熱鍵
/// </summary>
public class HotkeyService : IDisposable
{
    private const int HOTKEY_ID = 9000;

    private IntPtr _windowHandle;
    private HwndSource? _source;
    private bool _isRegistered;
    private bool _disposed;

    /// <summary>
    /// 熱鍵按下事件
    /// </summary>
    public event Action? HotkeyPressed;

    /// <summary>
    /// 註冊全域熱鍵
    /// </summary>
    /// <param name="hostWindow">宿主視窗 (用於接收 Windows 訊息)</param>
    /// <param name="modifiers">修飾鍵 (Ctrl, Alt, Shift, Win)</param>
    /// <param name="key">主要按鍵</param>
    /// <exception cref="InvalidOperationException">當熱鍵已被佔用時擲出</exception>
    public void Register(Window hostWindow, ModifierKeys modifiers, Key key)
    {
        if (_isRegistered)
        {
            throw new InvalidOperationException("熱鍵已經註冊");
        }

        // 取得視窗控制碼
        var helper = new WindowInteropHelper(hostWindow);
        _windowHandle = helper.EnsureHandle();

        // 取得 HwndSource 以接收 Windows 訊息
        _source = HwndSource.FromHwnd(_windowHandle);
        _source?.AddHook(HwndHook);

        // 轉換修飾鍵
        uint mod = 0;
        if (modifiers.HasFlag(ModifierKeys.Alt))
            mod |= NativeMethods.MOD_ALT;
        if (modifiers.HasFlag(ModifierKeys.Control))
            mod |= NativeMethods.MOD_CONTROL;
        if (modifiers.HasFlag(ModifierKeys.Shift))
            mod |= NativeMethods.MOD_SHIFT;
        if (modifiers.HasFlag(ModifierKeys.Windows))
            mod |= NativeMethods.MOD_WIN;

        // 轉換按鍵為虛擬鍵碼
        uint vk = (uint)KeyInterop.VirtualKeyFromKey(key);

        // 註冊熱鍵
        _isRegistered = NativeMethods.RegisterHotKey(_windowHandle, HOTKEY_ID, mod, vk);

        if (!_isRegistered)
        {
            _source?.RemoveHook(HwndHook);
            throw new InvalidOperationException("無法註冊熱鍵，可能已被其他應用程式佔用。");
        }
    }

    /// <summary>
    /// 取消註冊熱鍵
    /// </summary>
    public void Unregister()
    {
        if (_isRegistered)
        {
            NativeMethods.UnregisterHotKey(_windowHandle, HOTKEY_ID);
            _isRegistered = false;
        }

        _source?.RemoveHook(HwndHook);
    }

    /// <summary>
    /// Windows 訊息處理掛鉤
    /// </summary>
    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeMethods.WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
        {
            // 觸發熱鍵事件
            HotkeyPressed?.Invoke();
            handled = true;
        }

        return IntPtr.Zero;
    }

    /// <summary>
    /// 釋放資源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        Unregister();
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 解構函式
    /// </summary>
    ~HotkeyService()
    {
        Dispose();
    }
}
