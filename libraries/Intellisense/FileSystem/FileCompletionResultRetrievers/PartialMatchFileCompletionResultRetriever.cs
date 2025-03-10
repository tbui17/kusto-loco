namespace Intellisense.FileSystem.FileCompletionResultRetrievers;

internal class PartialMatchFileCompletionResultRetriever(
    IFileSystemReader reader
    ) : FileCompletionResultRetriever
{
    internal override CompletionResult GetCompletionResult(string path)
    {
        if (ParentChildPathPair.Create(path) is not { } pair)
        {
            return CompletionResult.Empty;
        }

        if (!reader.Exists(pair.ParentPath))
        {
            return CompletionResult.Empty;
        }

        var entries = reader.Read(pair.ParentPath)
            .Where(x => x.Name.Contains(pair.CurrentPath, StringComparison.CurrentCultureIgnoreCase));

        return new CompletionResult
        {
            Entries = entries,
            Rewind = pair.CurrentPath.Length
        };
    }
}
