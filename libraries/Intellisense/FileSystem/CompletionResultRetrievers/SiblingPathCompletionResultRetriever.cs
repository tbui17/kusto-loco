using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

/// <summary>
/// Retrieves the siblings of a directory or file and adds filtering data based on the name.
/// </summary>
internal class SiblingPathCompletionResultRetriever(IFileSystemReader reader)
    : IFileSystemPathCompletionResultRetriever
{
    public async Task<CompletionResult> GetCompletionResultAsync(IFileSystemPath fileSystemPath)
    {
        await Task.CompletedTask;
        var path = fileSystemPath.GetPath();

        var pair = ParentChildPathPair.Create(path);

        return reader
                .GetChildren(pair.ParentPath)
                .ToCompletionResult() with
            {
                Filter = pair.CurrentPath
            };
    }
}
