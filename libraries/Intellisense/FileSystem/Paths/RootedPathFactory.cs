namespace Intellisense.FileSystem.Paths;

internal interface IRootedPathFactory
{
    IFileSystemPath Create(string path);
}

internal class RootedPathFactory : IRootedPathFactory
{
    public IFileSystemPath Create(string path)
    {
        // generated IL will change with platform https://github.com/dotnet/runtime/blob/1b090a983e590275cccd92a127a07b4b80e80365/src/libraries/System.Private.CoreLib/src/System/IO/Path.Unix.cs#L124
        if (!Path.IsPathRooted(path))
        {
            return EmptyPath.Instance;
        }

        // we only care about windows or unix
        if (OperatingSystem.IsWindows())
        {
            return new WindowsRootedPath(path);
        }

        return new UnixRootedPath(path);
    }

    public T CreateOrThrow<T>(string path) where T : IFileSystemPath
    {
        var res = Create(path);
        if (res is not T rootedPath)
        {
            throw new ArgumentException($"Failed to create {nameof(T)} from {path}. OS: {Environment.OSVersion}");
        }

        return rootedPath;
    }
}
