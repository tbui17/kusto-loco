using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Intellisense.FileSystem.Shares;
using IntellisenseTests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntellisenseTests;

public class HostRepositoryTests
{
    [Fact]
    public async Task ListAsync_RetrievesMostRecentlyUsedEntries()
    {
        var fixture = new DatabaseFixture();
        var service = fixture.Provider.GetRequiredService<IHostRepository>();
        foreach (var host in Enumerable.Range(0, 200).Select(x => $"host{x}"))
        {
            await service.AddAsync(host);
        }

        var result = await service.ListAsync();

        result.Should().NotContain("host0").And.Contain("host199").And.HaveCount(100);
    }
}
