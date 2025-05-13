using Intellisense.FileSystem.Shares;
using Microsoft.EntityFrameworkCore;

namespace Intellisense;

internal class IntellisenseDbContext(DbContextOptions<IntellisenseDbContext> opts) : DbContext(opts)
{
    public DbSet<HostName> Hosts { get; set; }
    private const int LongMaxPath = 32767;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
        // ReSharper disable once ArrangeMethodOrOperatorBody
    {
        modelBuilder.Entity<HostName>(e =>
            {
                e.HasIndex(p => p.Name).IsUnique();
                e.Property(p => p.Name).HasMaxLength(LongMaxPath);
                e.Property(p => p.Name).IsRequired();
                e.HasIndex(p => p.Updated).IsDescending();
            }
        );
    }
}