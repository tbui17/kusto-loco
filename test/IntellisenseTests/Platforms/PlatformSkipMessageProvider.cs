using System.Runtime.InteropServices;

namespace IntellisenseTests.Platforms;

public static class PlatformSkipMessageProvider
{
    public static string GetWindowsSkipMessage() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? string.Empty
        : "Windows tests are not run on non-Windows platforms";
}
