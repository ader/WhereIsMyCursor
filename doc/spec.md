# WhereIsMyCursor 完整規格書

## 1. 專案概述

### 1.1 目的
開發一個 Windows 桌面應用程式，幫助使用者快速定位滑鼠游標位置。透過視覺效果（箭頭動畫、螢光脈衝）與動態系統托盤圖示，讓使用者能輕鬆找到游標。

### 1.2 技術規格
| 項目 | 規格 |
|------|------|
| 框架 | .NET 8 |
| UI 框架 | WPF (Windows Presentation Foundation) |
| 語言 | C# |
| 目標平台 | Windows 10/11 |
| 專案名稱 | WhereIsMyCursor |

---

## 2. 功能規格

### 2.1 箭頭指標效果

#### 2.1.1 行為描述
當使用者觸發尋找游標功能時，從螢幕四個角落同時發射箭頭，沿直線飛向游標位置。

#### 2.1.2 視覺規格
| 屬性 | 規格 |
|------|------|
| 箭頭形狀 | 三角形指標 (Path 幾何: `M 15,0 L 30,40 L 15,30 L 0,40 Z`) |
| 尺寸 | 30px (寬) × 40px (高) |
| 填充色 | 橘紅色 (#FF6347 / Tomato) |
| 描邊色 | 白色 (#FFFFFF) |
| 描邊寬度 | 2px |

#### 2.1.3 動畫規格
| 屬性 | 規格 |
|------|------|
| 起點 | 螢幕/虛擬螢幕四個角落 |
| 終點 | 游標目前位置 |
| 飛行時間 | 300ms |
| 緩動函數 | QuadraticEase (EaseOut) |
| 旋轉 | 箭頭頭部指向游標方向 |
| 淡出時間 | 200ms (到達後) |

---

### 2.2 螢光效果

#### 2.2.1 行為描述
在游標位置顯示同心圓脈衝光環，從中心向外擴散 2-3 圈後消失。

#### 2.2.2 視覺規格
| 屬性 | 規格 |
|------|------|
| 形狀 | 圓形 (Ellipse) |
| 圈數 | 3 圈 |
| 起始大小 | 20px 直徑 |
| 結束大小 | 200px 直徑 |
| 顏色 | 亮藍色 (#00BFFF / DeepSkyBlue) |
| 漸層 | RadialGradientBrush (中心實色 → 邊緣透明) |
| 描邊寬度 | 4px |

#### 2.2.3 動畫規格
| 屬性 | 規格 |
|------|------|
| 擴散時間 | 800ms (每圈) |
| 圈間延遲 | 200ms |
| 總持續時間 | 約 1400ms (3圈交錯) |
| 淡出 | Opacity 1.0 → 0.0 (隨擴散同步) |
| 緩動函數 | QuadraticEase (EaseOut) |

---

### 2.3 動態托盤箭頭圖示

#### 2.3.1 行為描述
系統托盤圖示實時顯示一個指向游標位置的箭頭，箭頭角度隨游標移動而旋轉。同時以顏色和線條粗細表示游標與托盤的距離。

#### 2.3.2 圖示規格
| 屬性 | 規格 |
|------|------|
| 尺寸 | 16×16 px (標準系統托盤尺寸) |
| 繪製方式 | System.Drawing 動態生成 |
| 背景 | 透明 |
| 抗鋸齒 | 啟用 (SmoothingMode.AntiAlias) |

#### 2.3.3 距離視覺指示
以 10 級漸層顏色表示游標與托盤圖示的距離，從紅色 (最近) 漸變至綠色 (最遠)。

| 級別 | 距離範圍 | 箭頭顏色 | 線條粗細 |
|------|----------|----------|----------|
| 0 | 0 - 400px | 紅色 (#FF0000) | 3px |
| 1 | 400 - 800px | 紅橘色 (#FF2D00) | 3px |
| 2 | 800 - 1200px | 橘紅色 (#FF5A00) | 3px |
| 3 | 1200 - 1600px | 橘色 (#FF8700) | 3px |
| 4 | 1600 - 2000px | 橘黃色 (#FFB400) | 3px |
| 5 | 2000 - 2400px | 黃色 (#FFE100) | 3px |
| 6 | 2400 - 2800px | 黃綠色 (#D4F000) | 3px |
| 7 | 2800 - 3200px | 淺綠色 (#A0FF00) | 3px |
| 8 | 3200 - 3600px | 綠色 (#6CFF00) | 3px |
| 9 | ≥ 3600px | 亮綠色 (#48FF00) | 3px |

**顏色計算公式：**
- 起點顏色 (最近)：紅色 RGB(255, 0, 0)
- 終點顏色 (最遠)：綠色 RGB(72, 255, 0)
- 最遠距離閾值：4000px
- 漸層級數：10 級
- 線條粗細：固定 3px

#### 2.3.4 更新頻率設定
| 選項 | 更新間隔 | FPS | 說明 |
|------|----------|-----|------|
| 低 | 500ms | 2 | 省資源，箭頭變化較跳躍 |
| 中 (預設) | 100ms | 10 | 平衡流暢度與效能 |
| 高 | 33ms | 30 | 非常流暢，較耗資源 |

#### 2.3.5 啟用/停用
- 可在設定中開關此功能
- 停用時顯示靜態預設圖示

---

### 2.4 觸發方式

#### 2.4.1 全域熱鍵
| 屬性 | 規格 |
|------|------|
| 組合鍵 | Ctrl + Alt + F |
| 實作方式 | Win32 API (RegisterHotKey / UnregisterHotKey) |
| 訊息處理 | WM_HOTKEY (0x0312) |
| 衝突處理 | 若熱鍵已被佔用，顯示錯誤訊息 |

#### 2.4.2 托盤互動
| 動作 | 效果 |
|------|------|
| 雙擊托盤圖示 | 觸發尋找游標效果 |
| 右鍵選單「尋找游標」 | 觸發尋找游標效果 |

---

### 2.5 系統托盤

#### 2.5.1 行為
- 程式啟動後直接最小化到系統托盤
- 不顯示主視窗
- 托盤圖示常駐

#### 2.5.2 右鍵選單
| 選項 | 快捷提示 | 功能 |
|------|----------|------|
| 尋找游標 | (Ctrl+Alt+F) | 手動觸發尋找效果 |
| 設定... | | 開啟設定視窗 |
| 結束 | | 退出程式 |

#### 2.5.3 設定視窗內容
| 設定項目 | 類型 | 預設值 |
|----------|------|--------|
| 動態圖示更新頻率 | 下拉選單 (低/中/高) | 中 |
| 啟用動態托盤圖示 | 核取方塊 | 啟用 |

---

### 2.6 覆蓋視窗

#### 2.6.1 視窗屬性
| 屬性 | 值 | 說明 |
|------|-----|------|
| WindowStyle | None | 無邊框 |
| AllowsTransparency | True | 允許透明 |
| Background | Transparent | 完全透明背景 |
| Topmost | True | 置頂顯示 |
| ShowInTaskbar | False | 不顯示在工作列 |
| ShowActivated | False | 不搶奪焦點 |

#### 2.6.2 視窗擴展樣式 (Win32)
| 樣式 | 值 | 說明 |
|------|-----|------|
| WS_EX_TRANSPARENT | 0x00000020 | 滑鼠穿透 |
| WS_EX_LAYERED | 0x00080000 | 分層視窗 |

#### 2.6.3 多螢幕支援
| 屬性 | 值 |
|------|-----|
| Left | SystemParameters.VirtualScreenLeft |
| Top | SystemParameters.VirtualScreenTop |
| Width | SystemParameters.VirtualScreenWidth |
| Height | SystemParameters.VirtualScreenHeight |

覆蓋所有連接的螢幕，包含負座標區域（左側/上方螢幕）。

---

## 3. 技術規格

### 3.1 DPI 感知

#### 3.1.1 Manifest 設定
```xml
<application xmlns="urn:schemas-microsoft-com:asm.v3">
  <windowsSettings>
    <dpiAware xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">true/pm</dpiAware>
    <dpiAwareness xmlns="http://schemas.microsoft.com/SMI/2016/WindowsSettings">PerMonitorV2</dpiAwareness>
  </windowsSettings>
</application>
```

#### 3.1.2 座標轉換
- 游標位置使用 `GetCursorPos` 取得實體像素
- 轉換為 WPF 裝置獨立單位 (DIU)
- 使用 `PresentationSource.CompositionTarget.TransformFromDevice`

---

### 3.2 Win32 API 清單

| API | 來源 | 用途 |
|-----|------|------|
| RegisterHotKey | user32.dll | 註冊全域熱鍵 |
| UnregisterHotKey | user32.dll | 取消註冊熱鍵 |
| GetCursorPos | user32.dll | 取得游標位置 |
| GetWindowLong | user32.dll | 取得視窗樣式 |
| SetWindowLong | user32.dll | 設定視窗樣式 (click-through) |
| Shell_NotifyIconGetRect | shell32.dll | 取得托盤圖示位置 |
| DestroyIcon | user32.dll | 釋放動態生成的圖示 |

---

### 3.3 資源管理

#### 3.3.1 釋放清單
程式結束時必須釋放：
1. `NotifyIcon` - 系統托盤圖示
2. `RegisterHotKey` - 全域熱鍵
3. `DispatcherTimer` - 動態圖示更新計時器
4. `Icon` - 動態生成的圖示資源 (呼叫 `DestroyIcon`)

#### 3.3.2 釋放時機
- `Application.OnExit` 事件
- 實作 `IDisposable` 介面

---

## 4. 檔案結構

```
src/WhereIsMyCursor/
├── WhereIsMyCursor.csproj          # 專案檔
├── app.manifest                     # DPI 感知設定
├── App.xaml                         # 應用程式定義
├── App.xaml.cs                      # 應用程式進入點、生命週期管理
├── MainWindow.xaml                  # 隱藏視窗 (XAML)
├── MainWindow.xaml.cs               # 熱鍵宿主
│
├── Views/
│   ├── OverlayWindow.xaml           # 覆蓋層視窗 (XAML)
│   ├── OverlayWindow.xaml.cs        # 動畫邏輯
│   ├── SettingsWindow.xaml          # 設定視窗 (XAML)
│   └── SettingsWindow.xaml.cs       # 設定邏輯
│
├── Models/
│   └── AppSettings.cs               # 設定模型類別
│
├── Services/
│   ├── HotkeyService.cs             # 全域熱鍵服務
│   ├── CursorService.cs             # 游標位置服務
│   └── TrayIconService.cs           # 動態托盤圖示服務
│
├── Interop/
│   └── NativeMethods.cs             # Win32 P/Invoke 宣告
│
└── Resources/
    └── app.ico                      # 應用程式圖示 (備用靜態圖示)
```

---

## 5. 類別設計

### 5.1 App (App.xaml.cs)
```
責任：應用程式生命週期、托盤管理
屬性：
  - _notifyIcon: NotifyIcon
  - _mainWindow: MainWindow
  - _trayIconService: TrayIconService
方法：
  + OnStartup(): void
  + OnExit(): void
  - CreateContextMenu(): ContextMenuStrip
  - TriggerCursorFind(): void
  - OpenSettings(): void
  - ExitApplication(): void
```

### 5.2 MainWindow (MainWindow.xaml.cs)
```
責任：熱鍵宿主視窗 (隱藏)
屬性：
  - _hotkeyService: HotkeyService
方法：
  + OnSourceInitialized(): void
  + OnClosed(): void
```

### 5.3 OverlayWindow (Views/OverlayWindow.xaml.cs)
```
責任：顯示視覺效果
屬性：
  - EffectCanvas: Canvas
方法：
  + ShowAndAnimate(): void
  - SetupWindowBounds(): void
  - MakeClickThrough(): void
  - CreateAndAnimateArrows(Point cursorPos): void
  - CreateGlowEffect(Point cursorPos): void
  - CreateArrow(): Path
```

### 5.4 HotkeyService (Services/HotkeyService.cs)
```
責任：全域熱鍵註冊與處理
屬性：
  - _windowHandle: IntPtr
  - _source: HwndSource
  - _isRegistered: bool
事件：
  + HotkeyPressed: Action
方法：
  + Register(Window, ModifierKeys, Key): void
  + Unregister(): void
  + Dispose(): void
  - HwndHook(...): IntPtr
```

### 5.5 TrayIconService (Services/TrayIconService.cs)
```
責任：動態托盤圖示生成與更新
屬性：
  - _updateTimer: DispatcherTimer
  - _notifyIcon: NotifyIcon
  - _previousIcon: Icon
  + UpdateIntervalMs: int
  + IsEnabled: bool
方法：
  + StartTracking(): void
  + StopTracking(): void
  + Dispose(): void
  - OnTimerTick(): void
  - GenerateArrowIcon(Point cursorPos): Icon
  - GetTrayIconPosition(): Point
  - CalculateAngle(Point from, Point to): double
  - GetDistanceStyle(double distance): (Color, float)
```

### 5.6 CursorService (Services/CursorService.cs)
```
責任：取得游標位置
方法：
  + GetCursorPosition(): Point (WPF DIU)
  + GetCursorPositionRaw(): Point (實體像素)
```

### 5.7 AppSettings (Models/AppSettings.cs)
```
責任：儲存應用程式設定
屬性：
  + DynamicIconEnabled: bool = true
  + UpdateFrequency: UpdateFrequencyLevel = Medium
方法：
  + Save(): void
  + Load(): AppSettings
```

### 5.8 NativeMethods (Interop/NativeMethods.cs)
```
責任：Win32 API 宣告
常數：
  + WM_HOTKEY = 0x0312
  + GWL_EXSTYLE = -20
  + WS_EX_TRANSPARENT = 0x00000020
  + WS_EX_LAYERED = 0x00080000
  + MOD_ALT = 0x0001
  + MOD_CONTROL = 0x0002
結構：
  + POINT { X, Y }
  + RECT { Left, Top, Right, Bottom }
  + NOTIFYICONIDENTIFIER { ... }
方法：
  + RegisterHotKey(...)
  + UnregisterHotKey(...)
  + GetCursorPos(...)
  + GetWindowLong(...)
  + SetWindowLong(...)
  + Shell_NotifyIconGetRect(...)
  + DestroyIcon(...)
```

---

## 6. 實作順序

### Phase 1: 專案基礎
1. 建立 .NET 8 WPF 專案
2. 設定 app.manifest (DPI)
3. 實作 NativeMethods.cs
4. 實作 CursorService.cs

### Phase 2: 系統托盤
1. 在 App.xaml.cs 設定 NotifyIcon
2. 建立右鍵選單
3. 設定 ShutdownMode

### Phase 3: 全域熱鍵
1. 建立隱藏 MainWindow
2. 實作 HotkeyService
3. 連接觸發事件

### Phase 4: 覆蓋視窗
1. 建立 OverlayWindow (XAML)
2. 實作多螢幕定位
3. 實作 click-through

### Phase 5: 箭頭動畫
1. 實作箭頭幾何圖形
2. 實作旋轉計算
3. 實作飛行動畫
4. 實作淡出效果

### Phase 6: 螢光效果
1. 實作同心圓元素
2. 實作擴散動畫
3. 實作交錯啟動

### Phase 7: 動態托盤圖示
1. 實作 TrayIconService
2. 實作圖示繪製邏輯
3. 實作距離視覺指示
4. 加入計時器更新

### Phase 8: 設定功能
1. 建立 AppSettings 模型
2. 建立 SettingsWindow
3. 實作設定儲存/載入

### Phase 9: 整合與測試
1. 連接所有元件
2. 資源釋放驗證
3. 多螢幕測試
4. DPI 測試

---

## 7. 注意事項與限制

### 7.1 已知限制
- 動態托盤圖示更新過於頻繁可能影響效能
- `Shell_NotifyIconGetRect` 在某些情況下可能失敗，需備用方案

### 7.2 錯誤處理
- 熱鍵註冊失敗：顯示訊息提示使用者
- 托盤圖示位置無法取得：使用螢幕右下角作為預設值

### 7.3 相容性
- 需要 Windows 10 1607 以上版本 (PerMonitorV2 DPI)
- 需要 .NET 8 Runtime
