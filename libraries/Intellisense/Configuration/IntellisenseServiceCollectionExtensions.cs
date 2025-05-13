using System.IO.Abstractions;
using System.Reflection;
using System.Runtime.InteropServices;
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

        services.AddIntellisenseDatabase();


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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            services
                .AddScoped<IShareReader, Win32ApiShareReader>()
                .AddScoped<IHostReader, Win32ApiHostReader>();
        }
        else
        {
            services.AddSingleton<IHostReader, NullReader>();
        }

        services
            .AddScoped<IHostRepository, HostRepository>()
            .AddScoped<IConnectionVerifier, ConnectionVerifier>();


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
    public static IServiceCollection AddIntellisenseDatabase(this IServiceCollection services)
    {
        services
            .AddDbContextFactory<IntellisenseDbContext>((provider, builder) =>
                builder.UseSqlite(provider.GetRequiredService<IOptionsMonitor<IntellisenseOptions>>()
                    .CurrentValue.DatabaseLocation
                )
            )
            .AddSingleton<DbContextOptions<IntellisenseDbContext>>(provider =>
                {
                    var builder = new DbContextOptionsBuilder<IntellisenseDbContext>();
                    var connectionString =
                        $"Data Source={provider.GetRequiredService<IOptionsMonitor<IntellisenseOptions>>().CurrentValue.DatabaseLocation}";
                    builder.UseSqlite(connectionString);
                    return builder.Options;
                }
            )
            .AddScoped<IntellisenseDbContext>(x =>
                x.GetRequiredService<IDbContextFactory<IntellisenseDbContext>>().CreateDbContext()
            )
            .AddSingleton<IntellisenseDbManager>();

        return services;
    }
}
