using System.IO.Abstractions;

namespace Intellisense.FileSystem;

public interface IFileSystemReader
{
    public IEnumerable<IFileSystemInfo> GetChildren(string path);
    public bool Exists(string path);
    public bool IsDirectory(string path);
    public bool IsFile(string path);
    public bool IsRoot(string path);
}

public class FileSystemReader(IFileSystem fileSystem) : IFileSystemReader
{
    private static readonly EnumerationOptions EnumerationOptions = new()
    {
        IgnoreInaccessible = true
    };

    public IEnumerable<IFileSystemInfo> GetChildren(string path)
    {
        return fileSystem
            .DirectoryInfo.New(path)
            .EnumerateFileSystemInfos("*", EnumerationOptions);
    }

    public bool Exists(string path)
    {
        // https://github.com/TestableIO/System.IO.Abstractions/issues/1244
        return IsDirectory(path) || IsFile(path);
    }

    public bool IsDirectory(string path)
    {
        return fileSystem.DirectoryInfo.New(path).Exists;
    }

    public bool IsFile(string path)
    {
        return fileSystem.File.Exists(path);
    }

    public bool IsRoot(string path)
    {
        return Path.GetDirectoryName(path) is null && IsDirectory(path);
    }
}
