using System.Runtime.InteropServices;

namespace SlideShow;

internal static partial class NativeMethods
{
    [Flags]
    public enum EXECUTION_STATE : uint
    {
        ES_AWAYMODE_REQUIRED = 0x00000040,
        ES_CONTINUOUS = 0x80000000,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_SYSTEM_REQUIRED = 0x00000001
    }

    [LibraryImport("kernel32.dll", EntryPoint = "SetThreadExecutionState", SetLastError = true)]
    private static partial EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

    public static void KeepAwake()
    {
        SetThreadExecutionState(EXECUTION_STATE.ES_SYSTEM_REQUIRED);
    }

    public static void AllowSleep()
    {
        SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
    }
}
