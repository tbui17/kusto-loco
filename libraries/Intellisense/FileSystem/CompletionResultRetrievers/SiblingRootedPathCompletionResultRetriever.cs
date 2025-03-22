namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class SiblingRootedPathCompletionResultRetriever(IFileSystemReader reader) : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath fileSystemPath)
    {
        var pair = ParentChildPathPair.Create(fileSystemPath.GetPath());

        if (pair is { ParentPath: "", CurrentPath: "" })
        {
            return CompletionResult.Empty;
        }

        return reader
            .GetChildren(pair.ParentPath)
            .ToCompletionResult() with { Filter = pair.CurrentPath };
    }
}
