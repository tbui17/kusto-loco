using System.IO.Abstractions;

namespace Intellisense.FileSystem;

public interface ICompletionResultFactory
{
    public CompletionResult Create();
    public CompletionResult Create(string path);
    public CompletionResult Create(ParentChildPathPair parentChildPathPair);
}

internal class CompletionResultFactory(IFileSystemReader reader) : ICompletionResultFactory
{
    public CompletionResult Create()
    {
        return CompletionResult.Empty;
    }

    public CompletionResult Create(string path)
    {
        return new CompletionResult
        {
            Entries = reader.Read(path).Select(CreateEntry)
        };
    }

    public CompletionResult Create(ParentChildPathPair parentChildPathPair)
    {
        return new CompletionResult
        {
            Entries = reader.Read(parentChildPathPair.ParentPath).Select(CreateEntry),
            Rewind = parentChildPathPair.CurrentPath.Length
        };
    }

    private static IntellisenseEntry CreateEntry(IFileSystemInfo fileSystemInfo)
    {
        return new IntellisenseEntry { Name = fileSystemInfo.Name };
    }
}
