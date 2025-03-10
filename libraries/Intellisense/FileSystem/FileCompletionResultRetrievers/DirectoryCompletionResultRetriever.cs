using System.Diagnostics;

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
            throw new UnreachableException($"Did not expect to fail to retrieve dir and file name for path {path}");
        }

        return completionResultFactory.Create(pair);
    }
}
