using Intellisense.FileSystem.Paths;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

/// <summary>
/// Retrieves the children of a root or child directory ending with a directory separator.
/// </summary>
internal class ChildrenPathCompletionResultRetriever(IFileSystemReader reader)
    : IFileSystemPathCompletionResultRetriever
{
    public async Task<CompletionResult> GetCompletionResultAsync(IFileSystemPath fileSystemPath)
    {
        await Task.CompletedTask;
        var path = fileSystemPath.GetPath();
        if (!path.EndsWithDirectorySeparator())
        {
            return CompletionResult.Empty;
        }

        if (fileSystemPath.IsRootDirectory())
        {
            return reader.GetChildren(path).ToCompletionResult();
        }

        return reader
            .GetChildren(fileSystemPath.GetParent())
            .ToCompletionResult();
    }
}
