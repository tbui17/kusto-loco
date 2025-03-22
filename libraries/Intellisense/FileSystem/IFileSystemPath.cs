namespace Intellisense.FileSystem;

internal interface IFileSystemPath
{
    string GetPath();
    bool IsRootDirectory();
}

