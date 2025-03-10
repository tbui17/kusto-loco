using System.Diagnostics;

namespace Intellisense.FileSystem.FileCompletionResultRetrievers;

internal class DirFileCompletionResultRetriever(IFileSystemReader fileSystemReader)
    : FileCompletionResultRetriever
{
    internal override CompletionResult GetCompletionResult(string path)
    {
        if (Path.EndsInDirectorySeparator(path))
        {
            return new CompletionResult
            {
                Entries = fileSystemReader.Read(path)
            };
        }

        if (ParentChildPathPair.Create(path) is not { } pair)
        {
            throw new UnreachableException($"Did not expect to fail to retrieve dir and file name for path {path}");
        }

        return new CompletionResult
        {
            Entries = fileSystemReader.Read(pair.ParentPath),
            Rewind = pair.CurrentPath.Length
        };
    }
}
