namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class RootedPathCompletionResultRetriever(IEnumerable<IFileSystemPathCompletionResultRetriever> retrievers) : IPathCompletionResultRetriever
{

    public CompletionResult GetCompletionResult(string path)
    {
        var rootedPath = RootedPath.Create(path);

        return retrievers.Select(x => x.GetCompletionResult(rootedPath)).FirstOrDefault(x => x.Entries.Count > 0, CompletionResult.Empty);
    }
}
