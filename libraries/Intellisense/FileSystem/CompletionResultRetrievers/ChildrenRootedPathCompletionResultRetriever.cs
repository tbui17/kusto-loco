namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class ChildrenRootedPathCompletionResultRetriever(IFileSystemReader reader)
    : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath fileSystemPath)
    {

        var path = fileSystemPath.GetPath();
        if (!path.EndsWithDirectorySeparator() || Path.GetDirectoryName(path) is not { } parentDir)
        {
            return CompletionResult.Empty;
        }

        return reader
            .GetChildren(parentDir)
            .ToCompletionResult();
    }
}
