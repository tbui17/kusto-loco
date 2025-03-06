using System.IO.Abstractions;

namespace KustoLoco.Core.Intellisense;

public static class FileSystemIntellisenseServiceProvider
{
    private static readonly FileSystem FileSystem = new();
    public static IFileSystemIntellisenseService GetFileSystemIntellisenseService()
    {
        return new FileSystemIntellisenseServiceWithParser(new FileSystemIntellisenseService(FileSystem));
    }
}
