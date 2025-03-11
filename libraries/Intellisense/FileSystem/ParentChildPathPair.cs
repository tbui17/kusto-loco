namespace Intellisense.FileSystem;

internal record struct ParentChildPathPair
{
    public string ParentPath { get; }
    public string CurrentPath { get; }
    private ParentChildPathPair(string parentPath, string currentPath)
    {
        ParentPath = parentPath;
        CurrentPath = currentPath;
    }

    public static ParentChildPathPair? Create(string path)
    {
        if (Path.GetDirectoryName(path) is not { } dirName)
        {
            return null;
        }

        var fileName = Path.GetFileName(path);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        return new ParentChildPathPair(dirName, fileName);
    }
}
