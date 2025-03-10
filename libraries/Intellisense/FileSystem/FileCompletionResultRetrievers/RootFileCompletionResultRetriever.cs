namespace Intellisense.FileSystem.FileCompletionResultRetrievers;

internal class RootFileCompletionResultRetriever(ICompletionResultFactory completionResultFactory)
    : FileCompletionResultRetriever
{
    internal override CompletionResult GetCompletionResult(string path)
    {
        if (path.EndsWith(':'))
        {
            return completionResultFactory.Create();
        }

        return completionResultFactory.Create(path);
    }
}
