using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Intellisense.FileSystem.Shares;

internal interface IHostRepository
{
    Task<IEnumerable<string>> ListAsync();
    Task AddAsync(string host);
}

internal class HostRepository(
    TimeProvider timeProvider,
    ILogger<HostRepository> logger,
    HostDbContext db
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

    private void Migrate()
    {
        logger.LogInformation("Beginning migration.");
        try
        {
            db.Database.EnsureCreated();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to ensure database creation. Deleting and recreating database.");
            try
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
            catch (Exception e2)
            {
                logger.LogError(e2, "Failed to recreate database. Service will not provide host intellisense.");
            }
        }
    }
}

internal class HostName
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}

internal class HostDbContext(DbContextOptions<HostDbContext> opts) : DbContext(opts)
{
    public string Name { get; init; } = string.Empty;
    public DateTimeOffset Created { get; init; }
    public DbSet<HostName> Hosts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        // ReSharper disable once ArrangeMethodOrOperatorBody
    {
        modelBuilder.Entity<HostName>(e =>
            {
                e.HasIndex(p => p.Name).IsUnique();
                e.Property(p => p.Name).HasMaxLength(255);
            }
        );
    }
}
