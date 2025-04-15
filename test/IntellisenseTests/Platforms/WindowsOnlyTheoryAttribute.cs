using System.Runtime.InteropServices;
using Xunit;

namespace IntellisenseTests.Platforms;

public class WindowsOnlyTheoryAttribute : TheoryAttribute
{
    public override string Skip => RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? base.Skip
        : "Windows tests are not run on non-Windows platforms";
}
