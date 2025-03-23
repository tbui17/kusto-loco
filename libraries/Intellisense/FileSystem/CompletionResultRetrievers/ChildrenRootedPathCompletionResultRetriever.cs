namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class ChildrenRootedPathCompletionResultRetriever(IFileSystemReader reader)
    : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath rootedPath)
    {

        if (!rootedPath.GetPath().EndsWithDirectorySeparator() || rootedPath.GetPath().GetNonEmptyParentDirectory() is not { } parentDir)
        {
            return CompletionResult.Empty;
        }
        return reader
            .GetChildren(parentDir)
            .ToCompletionResult();
    }
}
