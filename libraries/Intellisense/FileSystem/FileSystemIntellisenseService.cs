using System.IO.Abstractions;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    CompletionResult GetPathIntellisenseOptions(string path);
}

public class FileSystemIntellisenseService(IFileSystem fileSystem) : IFileSystemIntellisenseService
{
    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        IgnoreInaccessible = true
    };

    public CompletionResult GetPathIntellisenseOptions(string path)
    {
        if (!fileSystem.Path.IsPathRooted(path))
        {
            return CompletionResult.Empty;
        }

        if (fileSystem.Directory.Exists(path))
        {
            if (path.EndsWith(':'))
            {
                return CompletionResult.Empty;
            }
            if (!fileSystem.Path.EndsInDirectorySeparator(path))
            {
                return CreateSingleEntryCompletionResult(path);
            }

            var result = GetOptionsFromFileSystem(path);

            return new CompletionResult
            {
                Entries = result
            };
        }

        if (fileSystem.Path.GetDirectoryName(path) is not { } dirPath)
        {
            return CompletionResult.Empty;
        }

        if (!fileSystem.Directory.Exists(dirPath))
        {
            return CompletionResult.Empty;
        }

        var fileName = fileSystem.Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return CompletionResult.Empty;
        }
        var entries = GetOptionsFromFileSystem(dirPath).Where(x => x.Name.StartsWith(fileName));

        return new CompletionResult
        {
            Entries = entries,
            Rewind = fileName.Length
        };
    }

    private CompletionResult CreateSingleEntryCompletionResult(string path)
    {
        var fileName = fileSystem.Path.GetFileName(path);
        return new CompletionResult
        {
            Entries = [new IntellisenseEntry {Name = fileName}],
            Rewind = fileName.Length
        };
    }

    private IEnumerable<IntellisenseEntry> GetOptionsFromFileSystem(string dirPath)
    {
        return fileSystem
            .DirectoryInfo.New(dirPath)
            .EnumerateFileSystemInfos("*", EnumerationOptions)
            .Select(x => new IntellisenseEntry { Name = x.Name });
    }
}
