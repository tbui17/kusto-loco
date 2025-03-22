namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class RootedPathCompletionResultRetriever(IEnumerable<IFileSystemPathCompletionResultRetriever> retrievers) : IPathCompletionResultRetriever
{

    public CompletionResult GetCompletionResult(string fileSystemPath)
    {
        var rootedPath = RootedPath.Create(fileSystemPath);

        return retrievers.Select(x => x.GetCompletionResult(rootedPath)).FirstOrDefault(x => x.Entries.Count > 0, CompletionResult.Empty);
    }
}
