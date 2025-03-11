using System.IO.Abstractions;

namespace Intellisense.FileSystem;

public interface ICompletionResultFactory
{
    public CompletionResult Create();
    public CompletionResult Create(string path);
    public CompletionResult Create(ParentChildPathPair parentChildPathPair);
    public CompletionResult Create(IFileName parentChildPathPair);
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
            Entries = reader.GetChildren(path).Select(CreateEntry)
        };
    }

    public CompletionResult Create(ParentChildPathPair parentChildPathPair)
    {
        return new FilterCompletionResult
        {
            Entries = reader.GetChildren(parentChildPathPair.ParentPath).Select(CreateEntry),
            Rewind = parentChildPathPair.CurrentPath.Length,
            Filter = parentChildPathPair.CurrentPath
        };
    }

    public CompletionResult Create(IFileName parentChildPathPair)
    {
        return new FilterCompletionResult
        {
            Entries = reader.GetChildren(parentChildPathPair.ParentPath).Select(CreateEntry),
            Rewind = parentChildPathPair.Name.Length,
            Filter = parentChildPathPair.Name
        };
    }

    private static IntellisenseEntry CreateEntry(IFileSystemInfo fileSystemInfo)
    {
        return new IntellisenseEntry { Name = fileSystemInfo.Name };
    }
}
