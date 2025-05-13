using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Intellisense;

internal class IntellisenseDbManager(
    DbContextOptions<IntellisenseDbContext> options,
    ILogger<IntellisenseDbManager> logger)
{

    public void Startup()
    {
        logger.LogTrace("Running startup.");
        using var migrationContext = new IntellisenseDbContext(options);
        Migrate(migrationContext);
        try
        {
            var hosts = migrationContext.Hosts.ToList();
            logger.LogInformation("Found {Count} hosts.", hosts.Count);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get hosts.");
        }
    }

    private void Migrate(DbContext db)
    {
        // for now we'll just wipe the db if it fails
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
