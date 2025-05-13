using Intellisense.FileSystem.Paths;
using Intellisense.FileSystem.Shares;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class SharePathCompletionResultRetriever(IShareService shareService)
    : IFileSystemPathCompletionResultRetriever
{
    public async Task<CompletionResult> GetCompletionResultAsync(IFileSystemPath fileSystemPath)
    {
        if (fileSystemPath is UncPath p)
        {
            var endsWithSep = p.GetPath().EndsWithDirectorySeparator();
            if (p.IsOnlyHost() && endsWithSep)
            {
                return (await shareService.GetSharesAsync(p.Host)).ToCompletionResult();
            }

            if (p.IsOnlyHostAndShare() && !endsWithSep)
            {
                return (await shareService.GetSharesAsync(p.Host)).ToCompletionResult() with
                {
                    Filter = p.Share
                };
            }
        }

        return CompletionResult.Empty;


    }
}
