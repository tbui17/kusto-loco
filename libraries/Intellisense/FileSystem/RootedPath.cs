namespace Intellisense.FileSystem;

public class RootedPath
{
    public string Value { get; }
    private RootedPath(string path)
    {
        Value = path;
    }
    public static RootedPath? Create(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return new RootedPath(path);
        }

        return null;
    }

    public static RootedPath CreateOrThrow(string path)
    {
        if (Path.IsPathRooted(path))
        {
            return new RootedPath(path);
        }

        throw new ArgumentException($"Attempted to create {nameof(RootedPath)} from {path} but it was not rooted.");
    }
}
