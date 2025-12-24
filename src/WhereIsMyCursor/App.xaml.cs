using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using WhereIsMyCursor.Models;
using WhereIsMyCursor.Services;
using WhereIsMyCursor.Views;
using Application = System.Windows.Application;

namespace WhereIsMyCursor;

/// <summary>
/// 應用程式主類別
/// 負責生命週期管理、系統托盤與動態圖示服務
/// </summary>
public partial class App : Application
{
    private NotifyIcon? _notifyIcon;
    private TrayIconService? _trayIconService;
    private AppSettings? _settings;

    /// <summary>
    /// 應用程式啟動
    /// </summary>
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 載入設定
        _settings = AppSettings.Load();

        // 設定系統托盤
        SetupNotifyIcon();

        // 啟動動態托盤圖示服務
        if (_settings.DynamicIconEnabled)
        {
            StartTrayIconService();
        }
    }

    /// <summary>
    /// 設定系統托盤圖示
    /// </summary>
    private void SetupNotifyIcon()
    {
        _notifyIcon = new NotifyIcon
        {
            Icon = CreateDefaultIcon(),
            Visible = true,
            Text = "WhereIsMyCursor - 游標位置指示器"
        };

        // 建立右鍵選單
        _notifyIcon.ContextMenuStrip = CreateContextMenu();
    }

    /// <summary>
    /// 建立預設圖示 (動態繪製箭頭)
    /// </summary>
    private Icon CreateDefaultIcon()
    {
        // 建立 16x16 的預設圖示
        using var bitmap = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        // 繪製向上箭頭
        using var pen = new Pen(Color.DeepSkyBlue, 2f);
        g.DrawLine(pen, 8, 2, 8, 14);   // 垂直線
        g.DrawLine(pen, 8, 2, 3, 7);    // 左斜線
        g.DrawLine(pen, 8, 2, 13, 7);   // 右斜線

        return Icon.FromHandle(bitmap.GetHicon());
    }

    /// <summary>
    /// 建立右鍵選單
    /// </summary>
    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();

        // 設定
        var settingsItem = new ToolStripMenuItem("設定...");
        settingsItem.Click += (s, e) => OpenSettings();
        menu.Items.Add(settingsItem);

        menu.Items.Add(new ToolStripSeparator());

        // 結束
        var exitItem = new ToolStripMenuItem("結束");
        exitItem.Click += (s, e) => ExitApplication();
        menu.Items.Add(exitItem);

        return menu;
    }

    /// <summary>
    /// 開啟設定視窗
    /// </summary>
    private void OpenSettings()
    {
        var settingsWindow = new SettingsWindow(_settings!);
        settingsWindow.SettingsChanged += OnSettingsChanged;
        settingsWindow.ShowDialog();
    }

    /// <summary>
    /// 設定變更事件處理
    /// </summary>
    private void OnSettingsChanged(AppSettings newSettings)
    {
        _settings = newSettings;
        _settings.Save();

        // 根據設定更新動態圖示服務
        if (_settings.DynamicIconEnabled)
        {
            StartTrayIconService();
        }
        else
        {
            StopTrayIconService();
        }
    }

    /// <summary>
    /// 啟動動態托盤圖示服務
    /// </summary>
    private void StartTrayIconService()
    {
        if (_trayIconService == null && _notifyIcon != null)
        {
            _trayIconService = new TrayIconService(_notifyIcon);
            _trayIconService.UpdateIntervalMs = _settings!.UpdateFrequency switch
            {
                UpdateFrequencyLevel.Low => 500,
                UpdateFrequencyLevel.Medium => 100,
                UpdateFrequencyLevel.High => 33,
                _ => 100
            };
            _trayIconService.OffsetX = _settings.TrayIconOffsetX;
            _trayIconService.OffsetY = _settings.TrayIconOffsetY;
            _trayIconService.StartTracking();
        }
        else if (_trayIconService != null)
        {
            // 更新頻率
            _trayIconService.UpdateIntervalMs = _settings!.UpdateFrequency switch
            {
                UpdateFrequencyLevel.Low => 500,
                UpdateFrequencyLevel.Medium => 100,
                UpdateFrequencyLevel.High => 33,
                _ => 100
            };
            // 更新偏移值
            _trayIconService.OffsetX = _settings.TrayIconOffsetX;
            _trayIconService.OffsetY = _settings.TrayIconOffsetY;
        }
    }

    /// <summary>
    /// 停止動態托盤圖示服務
    /// </summary>
    private void StopTrayIconService()
    {
        _trayIconService?.StopTracking();
        _trayIconService?.Dispose();
        _trayIconService = null;

        // 恢復預設圖示
        if (_notifyIcon != null)
        {
            _notifyIcon.Icon = CreateDefaultIcon();
        }
    }

    /// <summary>
    /// 退出應用程式
    /// </summary>
    private void ExitApplication()
    {
        // 停止服務
        StopTrayIconService();

        // 清理托盤圖示
        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        // 關閉應用程式
        Shutdown();
    }

    /// <summary>
    /// 應用程式退出
    /// </summary>
    protected override void OnExit(ExitEventArgs e)
    {
        // 確保資源釋放
        StopTrayIconService();

        if (_notifyIcon != null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }

        base.OnExit(e);
    }
}
