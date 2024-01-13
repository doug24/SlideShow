using System.Windows;
using System.Windows.Interop;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.System.Power;

namespace SlideShow;

internal static partial class Native
{
    internal static void KeepAwake()
    {
        PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED);
    }

    internal static void AllowSleep()
    {
        PInvoke.SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
    }

    unsafe internal static bool UseImmersiveDarkMode(Window window, bool enabled)
    {
        if (IsWindows10OrGreater(18985))
        {
            HWND hwnd = new(new WindowInteropHelper(window).Handle);
            BOOL dark = new(enabled);

            return 0 == PInvoke.DwmSetWindowAttribute(hwnd,
                DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE,
                &dark, sizeof(int));
        }

        return false;
    }

    private static bool IsWindows10OrGreater(int build = -1)
    {
        return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
    }
}
