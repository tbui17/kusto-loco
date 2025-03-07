using FluentAssertions;
using KustoLoco.Core.Intellisense;
using Xunit;

namespace CoreTests;

public class IntellisenseTests
{
    private readonly IntellisenseService _intellisenseService = new IntellisenseService();

    [Fact]
    public void ParseRootedPath_ValidPath_ReturnsPath()
    {
        var path = _intellisenseService.ParseRootedPath(".save C:/Users/MyUser");

        path.Should().Be("C:/Users/MyUser");
    }

    [Fact]
    public void ParseRootedPath_ValidPathNoIoQuery_ReturnsNull()
    {
        var path = _intellisenseService.ParseRootedPath("C:/Users/MyUser");

        path.Should().BeNull();
    }
}
