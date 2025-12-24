# WhereIsMyCursor 規格書

## 1. 專案概述

### 1.1 目的
開發一個 Windows 桌面應用程式，透過動態系統托盤圖示幫助使用者快速定位滑鼠游標位置。托盤圖示會實時顯示指向游標的箭頭，並以顏色表示距離遠近。

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

### 2.1 動態托盤箭頭圖示

#### 2.1.1 行為描述
系統托盤圖示實時顯示一個指向游標位置的箭頭，箭頭角度隨游標移動而旋轉。同時以顏色表示游標與托盤的距離。

#### 2.1.2 圖示規格
| 屬性 | 規格 |
|------|------|
| 尺寸 | 16×16 px (標準系統托盤尺寸) |
| 繪製方式 | System.Drawing 動態生成 |
| 背景 | 透明 |
| 抗鋸齒 | 啟用 (SmoothingMode.AntiAlias) |

#### 2.1.3 距離視覺指示
以 10 級固定顏色 + 閃爍效果表示游標與托盤圖示的距離。
- 顏色：從紅色 (最近) 到綠色 (最遠)
- 閃爍：近距離快閃，遠距離不閃
- 閃爍方式：透明度切換 (100% ↔ 30%)

| 級別 | 距離範圍 | 箭頭顏色 | 閃爍間隔 | 線條粗細 |
|------|----------|----------|----------|----------|
| 0 | 0 - 400px | 紅色 (#FF0000) | 150ms (快閃) | 3px |
| 1 | 400 - 800px | 紅橘色 (#FF2D00) | 200ms (快閃) | 3px |
| 2 | 800 - 1200px | 橘紅色 (#FF5A00) | 200ms (快閃) | 3px |
| 3 | 1200 - 1600px | 橘色 (#FF8700) | 300ms (快閃) | 3px |
| 4 | 1600 - 2000px | 橘黃色 (#FFB400) | 300ms (中閃) | 3px |
| 5 | 2000 - 2400px | 黃色 (#FFE100) | 400ms (中閃) | 3px |
| 6 | 2400 - 2800px | 黃綠色 (#D4F000) | 500ms (慢閃) | 3px |
| 7 | 2800 - 3200px | 淺綠色 (#A0FF00) | 700ms (慢閃) | 3px |
| 8 | 3200 - 3600px | 綠色 (#6CFF00) | 1000ms (極慢) | 3px |
| 9 | ≥ 3600px | 亮綠色 (#48FF00) | 不閃爍 | 3px |

#### 2.1.4 更新頻率設定
| 選項 | 更新間隔 | FPS | 說明 |
|------|----------|-----|------|
| 低 | 500ms | 2 | 省資源，箭頭變化較跳躍 |
| 中 (預設) | 100ms | 10 | 平衡流暢度與效能 |
| 高 | 33ms | 30 | 非常流暢，較耗資源 |

#### 2.1.5 啟用/停用
- 可在設定中開關此功能
- 停用時顯示靜態預設圖示

---

### 2.2 系統托盤

#### 2.2.1 行為
- 程式啟動後直接最小化到系統托盤
- 不顯示主視窗
- 托盤圖示常駐

#### 2.2.2 右鍵選單
| 選項 | 功能 |
|------|------|
| 設定... | 開啟設定視窗 |
| 結束 | 退出程式 |

#### 2.2.3 設定視窗內容
| 設定項目 | 類型 | 預設值 |
|----------|------|--------|
| 啟用動態托盤圖示 | 核取方塊 | 啟用 |
| 啟用閃爍效果 | 核取方塊 | 啟用 |
| 動態圖示更新頻率 | 下拉選單 (低/中/高) | 中 |
| X 偏移 (右→左) | 數值輸入 | 100 |
| Y 偏移 (上→下) | 數值輸入 | 0 |

---

## 3. 技術規格

### 3.1 Win32 API 清單

| API | 來源 | 用途 |
|-----|------|------|
| GetCursorPos | user32.dll | 取得游標位置 |
| DestroyIcon | user32.dll | 釋放動態生成的圖示 |

---

### 3.2 資源管理

#### 3.2.1 釋放清單
程式結束時必須釋放：
1. `NotifyIcon` - 系統托盤圖示
2. `DispatcherTimer` - 動態圖示更新計時器
3. `Icon` - 動態生成的圖示資源 (呼叫 `DestroyIcon`)

#### 3.2.2 釋放時機
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
│
├── Views/
│   └── SettingsWindow.xaml(.cs)    # 設定視窗
│
├── Models/
│   └── AppSettings.cs               # 設定模型類別
│
├── Services/
│   ├── CursorService.cs             # 游標位置服務
│   └── TrayIconService.cs           # 動態托盤圖示服務
│
└── Interop/
    └── NativeMethods.cs             # Win32 P/Invoke 宣告
```

---

## 5. 類別設計

### 5.1 App (App.xaml.cs)
```
責任：應用程式生命週期、托盤管理
屬性：
  - _notifyIcon: NotifyIcon
  - _trayIconService: TrayIconService
  - _settings: AppSettings
方法：
  + OnStartup(): void
  + OnExit(): void
  - CreateContextMenu(): ContextMenuStrip
  - OpenSettings(): void
  - ExitApplication(): void
```

### 5.2 TrayIconService (Services/TrayIconService.cs)
```
責任：動態托盤圖示生成與更新
屬性：
  - _updateTimer: DispatcherTimer
  - _notifyIcon: NotifyIcon
  - _previousIcon: Icon
  - _blinkElapsed: int (閃爍累計時間)
  - _isBlinkVisible: bool (閃爍顯示狀態)
  + UpdateIntervalMs: int
  + OffsetX: int
  + OffsetY: int
  + BlinkEnabled: bool
  + IsEnabled: bool
方法：
  + StartTracking(): void
  + StopTracking(): void
  + Dispose(): void
  - OnTimerTick(): void
  - UpdateIcon(): void
  - GenerateArrowIcon(double angle, double distance): Icon
  - GetTrayIconPosition(): Point
  - GetDistanceStyle(double distance): (Color, float)
  - GetBlinkInterval(double distance): int
  - UpdateBlinkState(int blinkInterval): void
  - GetCurrentOpacity(): float
```

### 5.3 CursorService (Services/CursorService.cs)
```
責任：取得游標位置
方法：
  + GetCursorPositionRaw(): Point (實體像素)
```

### 5.4 AppSettings (Models/AppSettings.cs)
```
責任：儲存應用程式設定
屬性：
  + DynamicIconEnabled: bool = true
  + BlinkEnabled: bool = true
  + UpdateFrequency: UpdateFrequencyLevel = Medium
  + TrayIconOffsetX: int = 100
  + TrayIconOffsetY: int = 0
方法：
  + Save(): void
  + Load(): AppSettings
  + Clone(): AppSettings
```

### 5.5 NativeMethods (Interop/NativeMethods.cs)
```
責任：Win32 API 宣告
結構：
  + POINT { X, Y }
方法：
  + GetCursorPos(...)
  + DestroyIcon(...)
```

---

## 6. 注意事項與限制

### 6.1 已知限制
- 動態托盤圖示更新過於頻繁可能影響效能
- 托盤圖示實際位置無法精確取得，需使用偏移值校正

### 6.2 錯誤處理
- 托盤圖示位置無法取得：使用螢幕右下角 + 偏移值作為預設值

### 6.3 相容性
- 需要 Windows 10/11
- 需要 .NET 8 Runtime
