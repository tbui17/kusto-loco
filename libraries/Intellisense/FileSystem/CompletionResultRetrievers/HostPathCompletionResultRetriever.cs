using Intellisense.FileSystem.Paths;
using Intellisense.FileSystem.Shares;


namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class HostPathCompletionResultRetriever(IHostRepository repository)
    : IFileSystemPathCompletionResultRetriever
{
    public async Task<CompletionResult> GetCompletionResultAsync(IFileSystemPath fileSystemPath)
    {
        if (fileSystemPath.GetPath() is "//" or @"\\")
        {
            var result = await repository.ListAsync();
            return result.ToCompletionResult();
        }

        if (fileSystemPath is UncPath p && p.IsHost() && !p.GetPath().EndsWithDirectorySeparator())
        {
            var result = await repository.ListAsync();
            return result.ToCompletionResult() with
            {
                Filter = p.Host
            };
        }

        return CompletionResult.Empty;
    }
}
