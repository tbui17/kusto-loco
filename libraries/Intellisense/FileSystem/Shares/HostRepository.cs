using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace Intellisense.FileSystem.Shares;

internal interface IHostRepository
{
    Task<IEnumerable<string>> ListAsync();
    Task AddAsync(string host);
}

internal class HostRepository(
    TimeProvider timeProvider,
    IntellisenseDbContext db
) : IHostRepository
{
    public async Task<IEnumerable<string>> ListAsync() => await db.Hosts.OrderByDescending(x => x.Updated).Take(100).Select(x => x.Name).ToListAsync();

    public async Task AddAsync(string host)
    {
        var hostName = CreateHostName(host);

        await db.Database.ExecuteSqlAsync(
            $"""
             INSERT INTO Hosts (Name, Created, Updated) 
             VALUES ({hostName.Name}, {hostName.Created}, {hostName.Updated})
             ON CONFLICT (Name) 
             DO UPDATE SET Updated = {hostName.Updated}
             """
        );
    }

    private HostName CreateHostName(string host)
    {
        var hostName = new HostName
        {
            Name = host.ToLowerInvariant(),
            Updated = timeProvider.GetUtcNow().DateTime,
            Created = timeProvider.GetUtcNow().DateTime
        };
        return hostName;
    }
}


internal class HostRepositoryProd(TimeProvider timeProvider) : IHostRepository
{
    private readonly ConcurrentDictionary<string, HostName> _hosts = new([]);

    public Task<IEnumerable<string>> ListAsync() => Task.FromResult<IEnumerable<string>>(_hosts.Keys);

    public async Task AddAsync(string host)
    {
        await Task.CompletedTask;
        var hostName = CreateHostName(host);
        _hosts.TryAdd(hostName.Name, hostName);
    }

    private HostName CreateHostName(string host)
    {
        var now = timeProvider.GetUtcNow().DateTime;
        var hostName = new HostName
        {
            Name = host.ToLowerInvariant(),
            Updated = now,
            Created = now,
        };
        return hostName;
    }
}