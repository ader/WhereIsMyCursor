using System.Windows;
using System.Windows.Controls;
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

        // 初始化控制項
        LoadSettings();
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
