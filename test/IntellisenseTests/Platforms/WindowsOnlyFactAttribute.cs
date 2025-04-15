using Xunit;

namespace IntellisenseTests.Platforms;

public class WindowsOnlyFactAttribute : FactAttribute
{
    public override string Skip => PlatformSkipMessageProvider.GetWindowsSkipMessage();
}
