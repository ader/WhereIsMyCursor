using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using WhereIsMyCursor.Models;

namespace WhereIsMyCursor.Views;

/// <summary>
/// 設定視窗
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;

    /// <summary>
    /// 設定變更事件
    /// </summary>
    public event Action<AppSettings>? SettingsChanged;

    /// <summary>
    /// 建構函式
    /// </summary>
    /// <param name="settings">目前設定</param>
    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();

        _settings = settings.Clone();

        // 設定視窗圖示
        SetWindowIcon();

        // 初始化控制項
        LoadSettings();
    }

    /// <summary>
    /// 設定視窗圖示
    /// </summary>
    private void SetWindowIcon()
    {
        using var bitmap = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        // 繪製 45 度角紅色箭頭 (右上方向)
        using var pen = new Pen(Color.Red, 2.5f)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round
        };
        g.DrawLine(pen, 3, 13, 13, 3);   // 主線
        g.DrawLine(pen, 13, 3, 7, 3);    // 箭頭上
        g.DrawLine(pen, 13, 3, 13, 9);   // 箭頭右

        var hIcon = bitmap.GetHicon();
        Icon = Imaging.CreateBitmapSourceFromHIcon(
            hIcon,
            Int32Rect.Empty,
            BitmapSizeOptions.FromEmptyOptions());
    }

    /// <summary>
    /// 載入設定到控制項
    /// </summary>
    private void LoadSettings()
    {
        // 設定核取方塊
        EnableBlinkCheckBox.IsChecked = _settings.BlinkEnabled;

        // 設定下拉選單
        foreach (ComboBoxItem item in FrequencyComboBox.Items)
        {
            if (item.Tag?.ToString() == _settings.UpdateFrequency.ToString())
            {
                FrequencyComboBox.SelectedItem = item;
                break;
            }
        }

        // 預設選擇
        if (FrequencyComboBox.SelectedItem == null)
        {
            FrequencyComboBox.SelectedIndex = 1; // Medium
        }

        // 設定偏移值
        OffsetXTextBox.Text = _settings.TrayIconOffsetX.ToString();
        OffsetYTextBox.Text = _settings.TrayIconOffsetY.ToString();
    }

    /// <summary>
    /// 從控制項取得設定
    /// </summary>
    private void SaveSettings()
    {
        _settings.BlinkEnabled = EnableBlinkCheckBox.IsChecked ?? true;

        if (FrequencyComboBox.SelectedItem is ComboBoxItem selectedItem)
        {
            var tag = selectedItem.Tag?.ToString();
            _settings.UpdateFrequency = tag switch
            {
                "Low" => UpdateFrequencyLevel.Low,
                "High" => UpdateFrequencyLevel.High,
                _ => UpdateFrequencyLevel.Medium
            };
        }

        // 儲存偏移值
        if (int.TryParse(OffsetXTextBox.Text, out int offsetX))
        {
            _settings.TrayIconOffsetX = offsetX;
        }
        if (int.TryParse(OffsetYTextBox.Text, out int offsetY))
        {
            _settings.TrayIconOffsetY = offsetY;
        }
    }

    /// <summary>
    /// 確定按鈕點擊
    /// </summary>
    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        SaveSettings();
        SettingsChanged?.Invoke(_settings);
        DialogResult = true;
        Close();
    }

    /// <summary>
    /// 取消按鈕點擊
    /// </summary>
    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
