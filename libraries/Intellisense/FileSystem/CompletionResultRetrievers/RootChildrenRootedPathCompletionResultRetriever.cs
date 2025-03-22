namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class RootChildrenRootedPathCompletionResultRetriever(IFileSystemReader reader)
    : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath fileSystemPath)
    {
        if (!fileSystemPath.IsRootDirectory())
        {
            return CompletionResult.Empty;
        }
        return reader.GetChildren(fileSystemPath.GetPath()).ToCompletionResult();
    }
}
