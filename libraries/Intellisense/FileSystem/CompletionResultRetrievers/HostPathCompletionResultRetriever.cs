using Intellisense.FileSystem.Paths;
using Intellisense.FileSystem.Shares;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class HostPathCompletionResultRetriever(IShareService shareService)
    : IFileSystemPathCompletionResultRetriever
{
    public async Task<CompletionResult> GetCompletionResultAsync(IFileSystemPath fileSystemPath)
    {
        var path = fileSystemPath.GetPath();

        if (path is "//" or @"\\")
        {
            return (await shareService.GetHostsAsync()).ToCompletionResult();
        }

        if (fileSystemPath is UncPath p && p.IsOnlyHost() && !path.EndsWithDirectorySeparator())
        {
            return (await shareService.GetHostsAsync()).ToCompletionResult() with
            {
                Filter = p.Host
            };
        }

        return CompletionResult.Empty;
    }
}
