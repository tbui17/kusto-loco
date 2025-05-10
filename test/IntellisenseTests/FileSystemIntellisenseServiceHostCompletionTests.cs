using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using Intellisense;
using Intellisense.FileSystem;
using IntellisenseTests.Fixtures;
using IntellisenseTests.Platforms;

namespace IntellisenseTests;

public class FileSystemIntellisenseServiceHostCompletionTests
{
    private readonly IFileSystemIntellisenseService _service = new DatabaseFixture().IntellisenseService;


    [WindowsAdminOnlyFact]
    public async Task GetPathIntellisenseOptions_AfterValidHostFound_ReturnsHost()
    {
        var twoSlash = "//";
        var localHost = $"//{Constants.LocalHost}/";

        var res1 = await _service.GetPathIntellisenseOptionsAsync(twoSlash);

        res1.Entries.Should().BeEmpty();

        await _service.GetPathIntellisenseOptionsAsync(localHost);

        var res2 = await _service.GetPathIntellisenseOptionsAsync(twoSlash);

        res2
            .Entries.Should()
            .ContainSingle()
            .Which.Name.Should()
            .Be(Constants.LocalHost);
    }

    [WindowsAdminOnlyFact]
    public async Task GetPathIntellisenseOptions_AfterValidHostFoundAndPartialName_ReturnsHost()
    {
        var twoSlash = "//";
        var localHost = $"//{Constants.LocalHost}/";
        var localH = $"//localh";

        var res1 = await _service.GetPathIntellisenseOptionsAsync(twoSlash);

        res1.Entries.Should().BeEmpty();

        await _service.GetPathIntellisenseOptionsAsync(localHost);

        var res2 = await _service.GetPathIntellisenseOptionsAsync(localH);

        res2
            .Entries.Should()
            .ContainSingle()
            .Which.Name.Should()
            .Be(Constants.LocalHost);
    }

    [WindowsAdminOnlyFact]
    public async Task GetPathIntellisenseOptions_PartialHost_ReturnsSiblings()
    {

        var localHost = $"//{Constants.LocalHost}/";
        var localH = $"//localh";

        var res1 = await _service.GetPathIntellisenseOptionsAsync(localH);

        res1.Entries.Should().BeEmpty();

        await _service.GetPathIntellisenseOptionsAsync(localHost);

        var res2 = await _service.GetPathIntellisenseOptionsAsync(localH);

        res2
            .Entries.Should()
            .ContainSingle()
            .Which.Name.Should()
            .Be(Constants.LocalHost);
    }

    [WindowsAdminOnlyFact]
    public async Task GetPathIntellisenseOptions_UncPathPartialIpAddress_ShowsAvailableHosts()
    {
        // setup cache state
        var loopbackIp = IPAddress.Loopback.ToString();
        var localHostWithSep = $"//{Constants.LocalHost}/";
        var loopbackIpWithSep = $"//{loopbackIp}/";
        var localHostNoSep = $"//{Constants.LocalHost}";
        var localHostNoSlashes = Constants.LocalHost;
        var loopbackIpNoSlashes = loopbackIp;

        await _service.GetPathIntellisenseOptionsAsync(localHostWithSep);
        await _service.GetPathIntellisenseOptionsAsync(loopbackIpWithSep);
        var res = await _service.GetPathIntellisenseOptionsAsync(localHostNoSep);

        using var _ = new AssertionScope();
        res.Entries.Select(x => x.Name).Should().Contain(loopbackIpNoSlashes).And.Contain(localHostNoSlashes);


        // check loopback works
        var partialLoopBackNoSep = "//127.0";
        var partialLoopBackNoSlashes = "127.0";

        var res2 = await _service.GetPathIntellisenseOptionsAsync(partialLoopBackNoSep);

        res2.Entries.Select(x => x.Name).Should().Contain(loopbackIpNoSlashes).And.Contain(localHostNoSlashes);
        res2.Filter.Should().Be(partialLoopBackNoSlashes);
    }
}