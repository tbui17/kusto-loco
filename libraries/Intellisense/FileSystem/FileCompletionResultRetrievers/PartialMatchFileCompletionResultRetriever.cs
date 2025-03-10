namespace Intellisense.FileSystem.FileCompletionResultRetrievers;

internal class PartialMatchFileCompletionResultRetriever(
    IFileSystemReader reader,
    ICompletionResultFactory completionResultFactory
    ) : IFileCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(RootedPath rootedPath)
    {
        var path = rootedPath.Value;
        if (ParentChildPathPair.Create(path) is not { } pair)
        {
            return completionResultFactory.Create();
        }

        if (!reader.Exists(pair.ParentPath))
        {
            return completionResultFactory.Create();
        }

        var result = completionResultFactory.Create(pair);

        return result with
        {
            Entries = result.Entries.Where(x => x.Name.Contains(pair.CurrentPath, StringComparison.CurrentCultureIgnoreCase))
        };
    }
}
