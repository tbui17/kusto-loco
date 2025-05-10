using System.IO.Abstractions;
using System.Reflection;
using Intellisense.FileSystem;
using Intellisense.FileSystem.CompletionResultRetrievers;
using Intellisense.FileSystem.Paths;
using Intellisense.FileSystem.Shares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace Intellisense.Configuration;

public static class IntellisenseServiceCollectionExtensions
{
    public static IServiceCollection AddIntellisense(this IServiceCollection services)
    {
        // main services
        services.AddSingleton<IFileSystemIntellisenseService, FileSystemIntellisenseService>();

        services.Configure<IntellisenseOptions>(x =>
            {
                // default location
                var folder1 = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var appName = Assembly.GetEntryAssembly()?.GetName().Name ?? "Unknown";
                x.Directory = Path.Combine(folder1, appName);
            }
        );


        // file system
        services.AddSingleton<IFileSystemReader, FileSystemReader>();

        // completion results
        services
            .AddScoped<IFileSystemPathCompletionResultRetriever, ChildrenPathCompletionResultRetriever>()
            .AddScoped<IFileSystemPathCompletionResultRetriever, SiblingPathCompletionResultRetriever>()
            .AddScoped<IFileSystemPathCompletionResultRetriever, HostPathCompletionResultRetriever>()
            .AddScoped<IFileSystemPathCompletionResultRetriever, SharePathCompletionResultRetriever>();

        // path processing
        services.AddSingleton<IPathFactory, PathFactory>();

        // shares

        services
            .AddScoped<IShareReader, Win32ApiShareReader>()
            .AddScoped<IHostRepository, HostRepository>()
            .AddScoped<IConnectionVerifier, ConnectionVerifier>()
            .AddSqliteDbContext<HostDbContext>();


        // timeouts
        services.AddCancellationContext();

        // auxiliary services
        services.TryAddSingleton<IFileSystem, System.IO.Abstractions.FileSystem>();
        services.TryAddSingleton(TimeProvider.System);


        return services;
    }
}

file static class DbContextServiceCollectionExtensions
{
    public static IServiceCollection AddSqliteDbContext<T>(this IServiceCollection services) where T : DbContext
    {
        services.AddDbContextFactory<T>((providers, opts) => opts
            .UseSqlite(
                $"Data Source={providers.GetRequiredService<IOptionsMonitor<IntellisenseOptions>>().CurrentValue.DatabaseLocation}"
            )
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        );
        return services;
    }
}
