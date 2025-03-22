namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class SiblingCompletionResultRetriever(IFileSystemReader reader) : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath fileSystemPath)
    {
        var pair = ParentChildPathPair.Create(fileSystemPath.GetPath());

        return reader
            .GetChildren(pair.ParentPath)
            .ToCompletionResult() with { Filter = pair.CurrentPath };
    }
}
