namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class RootChildrenRootedPathCompletionResultRetriever(IFileSystemReader reader)
    : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath rootedPath)
    {
        if (!rootedPath.IsRootDirectory())
        {
            return CompletionResult.Empty;
        }
        return reader.GetChildren(rootedPath.GetPath()).ToCompletionResult();
    }
}
