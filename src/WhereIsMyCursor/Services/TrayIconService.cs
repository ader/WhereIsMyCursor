using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Windows.Threading;
using WhereIsMyCursor.Interop;
using Point = System.Windows.Point;

namespace WhereIsMyCursor.Services;

/// <summary>
/// 動態托盤圖示服務
/// 實時顯示指向游標位置的箭頭，並以顏色表示距離
/// </summary>
public class TrayIconService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly DispatcherTimer _updateTimer;
    private Icon? _previousIcon;
    private bool _isTracking;
    private bool _disposed;

    // 距離閾值設定 (每級 400px)
    private const double DISTANCE_STEP = 400;

    /// <summary>
    /// 更新間隔 (毫秒)
    /// </summary>
    public int UpdateIntervalMs
    {
        get => (int)_updateTimer.Interval.TotalMilliseconds;
        set => _updateTimer.Interval = TimeSpan.FromMilliseconds(value);
    }

    /// <summary>
    /// 是否啟用
    /// </summary>
    public bool IsEnabled => _isTracking;

    /// <summary>
    /// X 偏移量 (從螢幕右邊緣往左的像素數)
    /// </summary>
    public int OffsetX { get; set; } = 100;

    /// <summary>
    /// Y 偏移量 (從工作列中心的像素數)
    /// </summary>
    public int OffsetY { get; set; } = 0;

    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="notifyIcon">系統托盤圖示</param>
    public TrayIconService(NotifyIcon notifyIcon)
    {
        _notifyIcon = notifyIcon;

        _updateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100) // 預設 10 FPS
        };
        _updateTimer.Tick += OnTimerTick;
    }

    /// <summary>
    /// 開始追蹤游標並更新圖示
    /// </summary>
    public void StartTracking()
    {
        if (_isTracking) return;

        _isTracking = true;
        _updateTimer.Start();
    }

    /// <summary>
    /// 停止追蹤
    /// </summary>
    public void StopTracking()
    {
        if (!_isTracking) return;

        _isTracking = false;
        _updateTimer.Stop();
    }

    /// <summary>
    /// 計時器觸發事件
    /// </summary>
    private void OnTimerTick(object? sender, EventArgs e)
    {
        try
        {
            UpdateIcon();
        }
        catch
        {
            // 忽略圖示更新錯誤，避免影響程式運行
        }
    }

    /// <summary>
    /// 更新托盤圖示
    /// </summary>
    private void UpdateIcon()
    {
        // 取得游標位置
        var cursorPos = CursorService.GetCursorPositionRaw();

        // 取得托盤圖示位置 (使用螢幕右下角作為備用)
        var trayPos = GetTrayIconPosition();

        // 計算角度和距離
        double dx = cursorPos.X - trayPos.X;
        double dy = cursorPos.Y - trayPos.Y;
        double angle = Math.Atan2(dy, dx) * (180 / Math.PI);
        double distance = Math.Sqrt(dx * dx + dy * dy);

        // 生成新圖示
        var newIcon = GenerateArrowIcon(angle, distance);

        // 更新托盤圖示
        var oldIcon = _notifyIcon.Icon;
        _notifyIcon.Icon = newIcon;

        // 釋放舊圖示
        if (_previousIcon != null)
        {
            NativeMethods.DestroyIcon(_previousIcon.Handle);
            _previousIcon.Dispose();
        }
        _previousIcon = newIcon;
    }

    /// <summary>
    /// 取得托盤圖示位置 (使用可調整的偏移量)
    /// </summary>
    private Point GetTrayIconPosition()
    {
        try
        {
            var screen = Screen.PrimaryScreen;
            if (screen != null)
            {
                var bounds = screen.Bounds;       // 螢幕完整區域
                var workArea = screen.WorkingArea; // 工作區域 (不含工作列)

                // 計算工作列位置和尺寸
                // 假設工作列在底部 (最常見情況)
                if (workArea.Bottom < bounds.Bottom)
                {
                    // 底部工作列
                    int taskbarHeight = bounds.Bottom - workArea.Bottom;
                    int taskbarCenterY = workArea.Bottom + (taskbarHeight / 2) + OffsetY;
                    int trayX = bounds.Right - OffsetX;
                    return new Point(trayX, taskbarCenterY);
                }
                else if (workArea.Top > bounds.Top)
                {
                    // 頂部工作列
                    int taskbarHeight = workArea.Top - bounds.Top;
                    int taskbarCenterY = bounds.Top + (taskbarHeight / 2) + OffsetY;
                    int trayX = bounds.Right - OffsetX;
                    return new Point(trayX, taskbarCenterY);
                }
                else if (workArea.Right < bounds.Right)
                {
                    // 右側工作列
                    int taskbarWidth = bounds.Right - workArea.Right;
                    int taskbarCenterX = workArea.Right + (taskbarWidth / 2) + OffsetX;
                    int trayY = bounds.Bottom - OffsetY;
                    return new Point(taskbarCenterX, trayY);
                }
                else if (workArea.Left > bounds.Left)
                {
                    // 左側工作列
                    int taskbarWidth = workArea.Left - bounds.Left;
                    int taskbarCenterX = bounds.Left + (taskbarWidth / 2) - OffsetX;
                    int trayY = bounds.Bottom - OffsetY;
                    return new Point(taskbarCenterX, trayY);
                }

                // 預設：螢幕右下角
                return new Point(bounds.Right - OffsetX, bounds.Bottom - 20 + OffsetY);
            }
        }
        catch
        {
            // 忽略錯誤
        }

        // 備用：使用螢幕右下角
        return new Point(
            System.Windows.SystemParameters.PrimaryScreenWidth - OffsetX,
            System.Windows.SystemParameters.PrimaryScreenHeight - 20 + OffsetY
        );
    }

    /// <summary>
    /// 根據距離取得樣式 (顏色和粗細)
    /// 使用 10 級固定顏色，從紅色 (最近) 到綠色 (最遠)
    /// </summary>
    private (Color color, float thickness) GetDistanceStyle(double distance)
    {
        Color color;

        if (distance < DISTANCE_STEP)           // 0 - 400px
            color = Color.FromArgb(0xFF, 0x00, 0x00);  // 紅色 #FF0000
        else if (distance < DISTANCE_STEP * 2)  // 400 - 800px
            color = Color.FromArgb(0xFF, 0x2D, 0x00);  // 紅橘色 #FF2D00
        else if (distance < DISTANCE_STEP * 3)  // 800 - 1200px
            color = Color.FromArgb(0xFF, 0x5A, 0x00);  // 橘紅色 #FF5A00
        else if (distance < DISTANCE_STEP * 4)  // 1200 - 1600px
            color = Color.FromArgb(0xFF, 0x87, 0x00);  // 橘色 #FF8700
        else if (distance < DISTANCE_STEP * 5)  // 1600 - 2000px
            color = Color.FromArgb(0xFF, 0xB4, 0x00);  // 橘黃色 #FFB400
        else if (distance < DISTANCE_STEP * 6)  // 2000 - 2400px
            color = Color.FromArgb(0xFF, 0xE1, 0x00);  // 黃色 #FFE100
        else if (distance < DISTANCE_STEP * 7)  // 2400 - 2800px
            color = Color.FromArgb(0xD4, 0xF0, 0x00);  // 黃綠色 #D4F000
        else if (distance < DISTANCE_STEP * 8)  // 2800 - 3200px
            color = Color.FromArgb(0xA0, 0xFF, 0x00);  // 淺綠色 #A0FF00
        else if (distance < DISTANCE_STEP * 9)  // 3200 - 3600px
            color = Color.FromArgb(0x6C, 0xFF, 0x00);  // 綠色 #6CFF00
        else                                     // >= 3600px
            color = Color.FromArgb(0x48, 0xFF, 0x00);  // 亮綠色 #48FF00

        return (color, 3f);
    }

    /// <summary>
    /// 生成箭頭圖示
    /// </summary>
    /// <param name="angle">箭頭角度 (度)</param>
    /// <param name="distance">與游標的距離</param>
    /// <returns>動態生成的圖示</returns>
    private Icon GenerateArrowIcon(double angle, double distance)
    {
        // 建立 16x16 點陣圖
        using var bitmap = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bitmap);

        // 啟用抗鋸齒
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        // 取得樣式
        var (color, thickness) = GetDistanceStyle(distance);

        // 移動原點到中心並旋轉
        g.TranslateTransform(8, 8);
        g.RotateTransform((float)angle);

        // 繪製箭頭 (指向右方，然後旋轉)
        using var pen = new Pen(color, thickness)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        // 箭頭主線
        g.DrawLine(pen, -5, 0, 5, 0);

        // 箭頭頭部
        g.DrawLine(pen, 2, -3, 5, 0);
        g.DrawLine(pen, 2, 3, 5, 0);

        // 轉換為圖示
        return Icon.FromHandle(bitmap.GetHicon());
    }

    /// <summary>
    /// 釋放資源
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        StopTracking();

        if (_previousIcon != null)
        {
            NativeMethods.DestroyIcon(_previousIcon.Handle);
            _previousIcon.Dispose();
            _previousIcon = null;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// 解構函式
    /// </summary>
    ~TrayIconService()
    {
        Dispose();
    }
}
