using System.Windows;
using System.Windows.Media;
using WhereIsMyCursor.Interop;
using Point = System.Windows.Point;
using Application = System.Windows.Application;

namespace WhereIsMyCursor.Services;

/// <summary>
/// 游標位置服務
/// 提供游標位置查詢功能，支援 DPI 轉換
/// </summary>
public static class CursorService
{
    /// <summary>
    /// 取得游標位置 (實體像素，未經 DPI 轉換)
    /// </summary>
    /// <returns>游標位置 (實體像素座標)</returns>
    public static Point GetCursorPositionRaw()
    {
        NativeMethods.GetCursorPos(out var point);
        return new Point(point.X, point.Y);
    }

    /// <summary>
    /// 取得游標位置 (WPF 裝置獨立單位)
    /// 已根據主螢幕 DPI 進行轉換
    /// </summary>
    /// <returns>游標位置 (WPF DIU 座標)</returns>
    public static Point GetCursorPosition()
    {
        NativeMethods.GetCursorPos(out var point);

        // 嘗試從目前應用程式取得 DPI 轉換矩陣
        var source = PresentationSource.FromVisual(Application.Current.MainWindow);
        if (source?.CompositionTarget != null)
        {
            var transform = source.CompositionTarget.TransformFromDevice;
            return transform.Transform(new Point(point.X, point.Y));
        }

        // 備用方案：使用 Graphics 取得 DPI
        return GetCursorPositionWithFallback(point.X, point.Y);
    }

    /// <summary>
    /// 取得游標位置 (使用指定的視覺元素進行 DPI 轉換)
    /// </summary>
    /// <param name="visual">用於取得 DPI 資訊的視覺元素</param>
    /// <returns>游標位置 (WPF DIU 座標)</returns>
    public static Point GetCursorPosition(Visual visual)
    {
        NativeMethods.GetCursorPos(out var point);

        var source = PresentationSource.FromVisual(visual);
        if (source?.CompositionTarget != null)
        {
            var transform = source.CompositionTarget.TransformFromDevice;
            return transform.Transform(new Point(point.X, point.Y));
        }

        return GetCursorPositionWithFallback(point.X, point.Y);
    }

    /// <summary>
    /// 備用 DPI 轉換方法
    /// </summary>
    private static Point GetCursorPositionWithFallback(int x, int y)
    {
        try
        {
            // 使用 System.Drawing 取得系統 DPI
            using var g = System.Drawing.Graphics.FromHwnd(IntPtr.Zero);
            double dpiX = g.DpiX / 96.0;
            double dpiY = g.DpiY / 96.0;
            return new Point(x / dpiX, y / dpiY);
        }
        catch
        {
            // 如果失敗，直接返回原始座標
            return new Point(x, y);
        }
    }
}
