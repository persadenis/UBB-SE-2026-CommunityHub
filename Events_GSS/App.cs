using System;

namespace Events_GSS
{
    // We recreate the class they were relying on
    public static class App
    {
        // This is the global variable all 33 files are trying to access
        public static IServiceProvider Services { get; set; } = null!;
        public static IntPtr MainWindowHandle { get; set; }
    }
}