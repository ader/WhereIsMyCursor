using System.Runtime.InteropServices;

namespace WhereIsMyCursor.Interop;

/// <summary>
/// Win32 API P/Invoke 宣告
/// </summary>
internal static class NativeMethods
{
    #region 常數定義

    // 視窗訊息
    public const int WM_HOTKEY = 0x0312;

    // 視窗樣式
    public const int GWL_EXSTYLE = -20;
    public const int WS_EX_TRANSPARENT = 0x00000020;  // 滑鼠穿透
    public const int WS_EX_LAYERED = 0x00080000;      // 分層視窗
    public const int WS_EX_TOOLWINDOW = 0x00000080;   // 工具視窗 (不顯示在工作列)

    // 熱鍵修飾鍵
    public const uint MOD_ALT = 0x0001;
    public const uint MOD_CONTROL = 0x0002;
    public const uint MOD_SHIFT = 0x0004;
    public const uint MOD_WIN = 0x0008;
    public const uint MOD_NOREPEAT = 0x4000;

    #endregion

    #region 結構定義

    /// <summary>
    /// 點座標結構
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct POINT
    {
        public int X;
        public int Y;
    }

    /// <summary>
    /// 矩形結構
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    /// <summary>
    /// 通知區域圖示識別碼結構
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NOTIFYICONIDENTIFIER
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uID;
        public Guid guidItem;
    }

    #endregion

    #region 熱鍵 API

    /// <summary>
    /// 註冊全域熱鍵
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    /// <summary>
    /// 取消註冊全域熱鍵
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    #endregion

    #region 游標 API

    /// <summary>
    /// 取得游標位置 (實體像素)
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool GetCursorPos(out POINT lpPoint);

    #endregion

    #region 視窗樣式 API

    /// <summary>
    /// 取得視窗擴展樣式 (32位元)
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    /// <summary>
    /// 設定視窗擴展樣式 (32位元)
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    /// <summary>
    /// 取得視窗擴展樣式 (64位元相容)
    /// </summary>
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "GetWindowLongPtr")]
    public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

    /// <summary>
    /// 設定視窗擴展樣式 (64位元相容)
    /// </summary>
    [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtr")]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    #endregion

    #region 托盤圖示 API

    /// <summary>
    /// 取得通知區域圖示的位置
    /// </summary>
    [DllImport("shell32.dll", SetLastError = true)]
    public static extern int Shell_NotifyIconGetRect([In] ref NOTIFYICONIDENTIFIER identifier, [Out] out RECT iconLocation);

    #endregion

    #region 圖示 API

    /// <summary>
    /// 銷毀圖示資源
    /// </summary>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static extern bool DestroyIcon(IntPtr hIcon);

    #endregion

    #region 輔助方法

    /// <summary>
    /// 跨平台設定視窗樣式 (自動判斷 32/64 位元)
    /// </summary>
    public static IntPtr SetWindowLongPtrSafe(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        if (IntPtr.Size == 8)
        {
            return SetWindowLongPtr(hWnd, nIndex, dwNewLong);
        }
        else
        {
            return new IntPtr(SetWindowLong(hWnd, nIndex, dwNewLong.ToInt32()));
        }
    }

    /// <summary>
    /// 跨平台取得視窗樣式 (自動判斷 32/64 位元)
    /// </summary>
    public static IntPtr GetWindowLongPtrSafe(IntPtr hWnd, int nIndex)
    {
        if (IntPtr.Size == 8)
        {
            return GetWindowLongPtr(hWnd, nIndex);
        }
        else
        {
            return new IntPtr(GetWindowLong(hWnd, nIndex));
        }
    }

    #endregion
}
