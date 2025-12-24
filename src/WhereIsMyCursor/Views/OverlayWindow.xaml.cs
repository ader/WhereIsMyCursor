using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using WhereIsMyCursor.Interop;
using WhereIsMyCursor.Services;
using Point = System.Windows.Point;
using Color = System.Windows.Media.Color;
using Brushes = System.Windows.Media.Brushes;

namespace WhereIsMyCursor.Views;

/// <summary>
/// 覆蓋視窗
/// 顯示箭頭指標和螢光效果動畫
/// </summary>
public partial class OverlayWindow : Window
{
    // 動畫計數器，用於追蹤動畫完成狀態
    private int _activeAnimations = 0;

    public OverlayWindow()
    {
        InitializeComponent();
        SetupWindowBounds();
    }

    /// <summary>
    /// 視窗來源初始化完成後設定滑鼠穿透
    /// </summary>
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        MakeClickThrough();
    }

    /// <summary>
    /// 設定視窗範圍 (覆蓋所有螢幕)
    /// </summary>
    private void SetupWindowBounds()
    {
        // 使用虛擬螢幕參數覆蓋所有螢幕
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;
    }

    /// <summary>
    /// 設定視窗為滑鼠穿透 (click-through)
    /// </summary>
    private void MakeClickThrough()
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var extStyle = NativeMethods.GetWindowLongPtrSafe(hwnd, NativeMethods.GWL_EXSTYLE);
        var newStyle = extStyle.ToInt64() | NativeMethods.WS_EX_TRANSPARENT | NativeMethods.WS_EX_LAYERED;
        NativeMethods.SetWindowLongPtrSafe(hwnd, NativeMethods.GWL_EXSTYLE, new IntPtr(newStyle));
    }

    /// <summary>
    /// 顯示視窗並開始動畫
    /// </summary>
    public void ShowAndAnimate()
    {
        Show();

        // 取得游標位置 (相對於覆蓋視窗)
        var cursorPos = CursorService.GetCursorPositionRaw();

        // 轉換為相對於視窗的座標
        var relativePos = new Point(
            cursorPos.X - Left,
            cursorPos.Y - Top
        );

        // 建立並播放箭頭動畫
        CreateAndAnimateArrows(relativePos);

        // 建立並播放螢光效果
        CreateGlowEffect(relativePos);
    }

    /// <summary>
    /// 建立並播放箭頭動畫
    /// </summary>
    private void CreateAndAnimateArrows(Point cursorPosition)
    {
        // 四個角落的座標
        Point[] corners = new Point[]
        {
            new Point(0, 0),                            // 左上
            new Point(Width, 0),                        // 右上
            new Point(0, Height),                       // 左下
            new Point(Width, Height)                    // 右下
        };

        foreach (var corner in corners)
        {
            // 建立箭頭
            var arrow = CreateArrow();
            EffectCanvas.Children.Add(arrow);

            // 計算旋轉角度 (箭頭指向游標)
            double dx = cursorPosition.X - corner.X;
            double dy = cursorPosition.Y - corner.Y;
            double angle = Math.Atan2(dy, dx) * (180 / Math.PI) + 90; // +90 因為箭頭預設朝上

            // 設定旋轉變換
            arrow.RenderTransform = new RotateTransform(angle, 15, 20); // 中心點 (30/2, 40/2)
            arrow.RenderTransformOrigin = new Point(0.5, 0.5);

            // 設定初始位置
            Canvas.SetLeft(arrow, corner.X - 15);
            Canvas.SetTop(arrow, corner.Y - 20);

            // 建立動畫
            var storyboard = new Storyboard();

            // X 軸移動動畫
            var animX = new DoubleAnimation
            {
                From = corner.X - 15,
                To = cursorPosition.X - 15,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(animX, arrow);
            Storyboard.SetTargetProperty(animX, new PropertyPath("(Canvas.Left)"));
            storyboard.Children.Add(animX);

            // Y 軸移動動畫
            var animY = new DoubleAnimation
            {
                From = corner.Y - 20,
                To = cursorPosition.Y - 20,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(animY, arrow);
            Storyboard.SetTargetProperty(animY, new PropertyPath("(Canvas.Top)"));
            storyboard.Children.Add(animY);

            // 淡出動畫 (到達後)
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                BeginTime = TimeSpan.FromMilliseconds(300),
                Duration = TimeSpan.FromMilliseconds(200)
            };
            Storyboard.SetTarget(fadeOut, arrow);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));
            storyboard.Children.Add(fadeOut);

            // 追蹤動畫
            _activeAnimations++;
            storyboard.Completed += (s, e) =>
            {
                EffectCanvas.Children.Remove(arrow);
                OnAnimationCompleted();
            };

            storyboard.Begin();
        }
    }

    /// <summary>
    /// 建立箭頭形狀
    /// </summary>
    private Path CreateArrow()
    {
        // 箭頭幾何圖形 (30x40px)
        // M 15,0 - 頂點
        // L 30,40 - 右下
        // L 15,30 - 中下凹槽
        // L 0,40 - 左下
        // Z - 閉合
        return new Path
        {
            Data = Geometry.Parse("M 15,0 L 30,40 L 15,30 L 0,40 Z"),
            Fill = new SolidColorBrush(Color.FromRgb(0xFF, 0x63, 0x47)), // #FF6347 (Tomato)
            Stroke = Brushes.White,
            StrokeThickness = 2,
            Width = 30,
            Height = 40,
            Stretch = Stretch.Fill
        };
    }

    /// <summary>
    /// 建立螢光效果
    /// </summary>
    private void CreateGlowEffect(Point cursorPosition)
    {
        int ringCount = 3;

        for (int i = 0; i < ringCount; i++)
        {
            // 建立光環
            var ring = new Ellipse
            {
                Width = 20,
                Height = 20,
                StrokeThickness = 4,
                Stroke = new SolidColorBrush(Color.FromArgb(255, 0, 191, 255)), // #00BFFF
                Fill = Brushes.Transparent,
                Opacity = 1.0
            };

            // 設定初始位置 (置中於游標)
            Canvas.SetLeft(ring, cursorPosition.X - 10);
            Canvas.SetTop(ring, cursorPosition.Y - 10);
            EffectCanvas.Children.Add(ring);

            // 建立動畫
            var storyboard = new Storyboard
            {
                BeginTime = TimeSpan.FromMilliseconds(i * 200) // 交錯啟動
            };

            // 寬度擴散動畫
            var scaleWidth = new DoubleAnimation
            {
                From = 20,
                To = 200,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleWidth, ring);
            Storyboard.SetTargetProperty(scaleWidth, new PropertyPath(WidthProperty));
            storyboard.Children.Add(scaleWidth);

            // 高度擴散動畫
            var scaleHeight = new DoubleAnimation
            {
                From = 20,
                To = 200,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(scaleHeight, ring);
            Storyboard.SetTargetProperty(scaleHeight, new PropertyPath(HeightProperty));
            storyboard.Children.Add(scaleHeight);

            // X 位置調整 (保持置中)
            var posX = new DoubleAnimation
            {
                From = cursorPosition.X - 10,
                To = cursorPosition.X - 100,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(posX, ring);
            Storyboard.SetTargetProperty(posX, new PropertyPath("(Canvas.Left)"));
            storyboard.Children.Add(posX);

            // Y 位置調整 (保持置中)
            var posY = new DoubleAnimation
            {
                From = cursorPosition.Y - 10,
                To = cursorPosition.Y - 100,
                Duration = TimeSpan.FromMilliseconds(800),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };
            Storyboard.SetTarget(posY, ring);
            Storyboard.SetTargetProperty(posY, new PropertyPath("(Canvas.Top)"));
            storyboard.Children.Add(posY);

            // 淡出動畫
            var fadeOut = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                Duration = TimeSpan.FromMilliseconds(800)
            };
            Storyboard.SetTarget(fadeOut, ring);
            Storyboard.SetTargetProperty(fadeOut, new PropertyPath(OpacityProperty));
            storyboard.Children.Add(fadeOut);

            // 追蹤動畫
            _activeAnimations++;
            storyboard.Completed += (s, e) =>
            {
                EffectCanvas.Children.Remove(ring);
                OnAnimationCompleted();
            };

            storyboard.Begin();
        }
    }

    /// <summary>
    /// 動畫完成處理
    /// </summary>
    private void OnAnimationCompleted()
    {
        _activeAnimations--;

        // 所有動畫完成後關閉視窗
        if (_activeAnimations <= 0)
        {
            Close();
        }
    }
}
