namespace Intellisense.FileSystem.FileCompletionResultRetrievers;

internal class RootFileCompletionResultRetriever(IFileSystemReader fileSystemReader)
    : FileCompletionResultRetriever
{
    internal override CompletionResult GetCompletionResult(string path)
    {
        if (path.EndsWith(':'))
        {
            return CompletionResult.Empty;
        }

        return new CompletionResult
        {
            Entries = fileSystemReader.Read(path)
        };
    }
}
