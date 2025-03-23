namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class SiblingRootedPathCompletionResultRetriever(IFileSystemReader reader) : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath rootedPath)
    {
        if (ParentChildPathPair.Create(rootedPath.GetPath()) is not { } pair)
        {
            return CompletionResult.Empty;
        }

        return reader
            .GetChildren(pair.ParentPath)
            .ToCompletionResult() with { Filter = pair.CurrentPath };
    }
}
