namespace Intellisense.FileSystem.FileCompletionResultRetrievers;

internal class DirectoryCompletionResultRetriever(ICompletionResultFactory completionResultFactory)
    : IFileCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(RootedPath rootedPath)
    {
        var path = rootedPath.Value;
        if (Path.EndsInDirectorySeparator(path))
        {
            return completionResultFactory.Create(path);
        }

        if (ParentChildPathPair.Create(path) is not { } pair)
        {
            return completionResultFactory.Create();
        }

        return completionResultFactory.Create(pair);
    }
}
