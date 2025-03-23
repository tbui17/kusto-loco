using Intellisense.FileSystem.Paths;
using NotNullStrings;

namespace Intellisense.FileSystem.CompletionResultRetrievers;

internal class ChildrenCompletionResultRetriever(IFileSystemReader reader)
    : IFileSystemPathCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(IFileSystemPath fileSystemPath)
    {

        var dir = GetTargetDirectory(fileSystemPath);

        return reader
            .GetChildren(dir)
            .ToCompletionResult();
    }

    private static string GetTargetDirectory(IFileSystemPath fileSystemPath)
    {
        var path = fileSystemPath.GetPath();
        if (!path.EndsWithDirectorySeparator())
        {
            return string.Empty;
        }

        if (fileSystemPath.IsRootDirectory())
        {
            return path;
        }

        return Path.GetDirectoryName(path).NullToEmpty();

    }
}
