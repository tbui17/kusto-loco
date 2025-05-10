using System.Runtime.InteropServices;
using Xunit;

namespace IntellisenseTests.Platforms;

public sealed class WindowsAdminOnlyFactAttribute : FactAttribute
{
    public WindowsAdminOnlyFactAttribute()
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        if (!isWindows)
        {
            Skip = PlatformHelper.WindowsSkipMessage;
            return;
        }

        var isAdmin = PlatformHelper.IsWindowsAdmin();

        if (isAdmin)
        {
            return;
        }

        Skip = PlatformHelper.AdminSkipMessage;
    }
}
