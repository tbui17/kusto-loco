namespace Intellisense.FileSystem.FileCompletionResultRetrievers;

internal class RootFileCompletionResultRetriever(ICompletionResultFactory completionResultFactory)
    : IFileCompletionResultRetriever
{
    public CompletionResult GetCompletionResult(string path)
    {
        if (path.EndsWith(':'))
        {
            return completionResultFactory.Create();
        }

        return completionResultFactory.Create(path);
    }
}
