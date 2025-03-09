using System.Diagnostics;
using System.IO.Abstractions;

namespace Intellisense.FileSystem;

public interface IFileSystemIntellisenseService
{
    CompletionResult GetPathIntellisenseOptions(RootedPath rootedPath);
}

public class FileSystemIntellisenseService(IFileSystem fileSystem) : IFileSystemIntellisenseService
{
    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        IgnoreInaccessible = true
    };

    public CompletionResult GetPathIntellisenseOptions(RootedPath rootedPath)
    {
        var path = rootedPath.Value;
        if (IsRoot(path))
        {
            return GetRootResults(path);
        }

        if (IsDirectory(path))
        {
            return GetDirectoryResults(path);
        }

        return GetParentResults(path);
    }

    private CompletionResult GetParentResults(string path)
    {
        if (GetDirAndFileNames(path) is not { } pair)
        {
            return CompletionResult.Empty;
        }

        var entries = GetOptionsFromFileSystem(pair.ParentPath)
            .Where(x => x.Name.Contains(pair.CurrentPath, StringComparison.CurrentCultureIgnoreCase));

        return new CompletionResult
        {
            Entries = entries,
            Rewind = pair.CurrentPath.Length
        };
    }

    private CompletionResult GetRootResults(string path)
    {
        if (path.EndsWith(':'))
        {
            return CompletionResult.Empty;
        }

        return new CompletionResult
        {
            Entries = GetOptionsFromFileSystem(path)
        };
    }

    private CompletionResult GetDirectoryResults(string path)
    {
        if (Path.EndsInDirectorySeparator(path))
        {
            return new CompletionResult
            {
                Entries = GetOptionsFromFileSystem(path)
            };
        }

        if (GetDirAndFileNames(path) is not { } pair)
        {
            throw new UnreachableException($"Did not expect to fail to retrieve dir and file name for path {path}");
        }

        return new CompletionResult
        {
            Entries = GetOptionsFromFileSystem(pair.ParentPath),
            Rewind = pair.CurrentPath.Length
        };
    }

    private bool IsDirectory(string path)
    {
        // https://github.com/TestableIO/System.IO.Abstractions/issues/1244
        return fileSystem.DirectoryInfo.New(path).Exists;
    }

    private (string ParentPath, string CurrentPath)? GetDirAndFileNames(string path)
    {
        if (Path.GetDirectoryName(path) is not { } dirName)
        {
            return null;
        }

        if (!IsDirectory(dirName))
        {
            return null;
        }

        var fileName = Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        return (dirName, fileName);
    }

    private bool IsRoot(string path)
    {
        return Path.GetDirectoryName(path) is null && IsDirectory(path);
    }

    private IEnumerable<IntellisenseEntry> GetOptionsFromFileSystem(string dirPath)
    {
        return fileSystem
            .DirectoryInfo.New(dirPath)
            .EnumerateFileSystemInfos("*", EnumerationOptions)
            .Select(x => new IntellisenseEntry { Name = x.Name });
    }
}
