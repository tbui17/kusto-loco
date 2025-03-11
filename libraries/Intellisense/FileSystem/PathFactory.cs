namespace Intellisense.FileSystem;

internal class PathFactory(IFileSystemReader reader)
{
    internal IFullPath CreatePath(RootedPath rootedPath)
    {
        var path = rootedPath.Value;

        if (reader.IsRoot(path))
        {
            return F1();
        }

        if (reader.IsDirectory(path))
        {
            return F2();
        }

        return F3();

        IFullPath F1()
        {
            if (path.EndsWith(':'))
            {
                return new RootPathEndingInColon(path);
            }
            return new RootPathNotEndingInColon(path);
        }


        IParentPath F2()
        {
            if (Path.EndsInDirectorySeparator(path))
            {
                return new ChildDirWithSep(path);
            }

            return new ChildDirWithoutSep(path);
        }

        IFullPath F3()
        {
            if (ParentChildPathPair.Create(path) is not { } pair)
            {
                return new NonExistentRootOrDirWithSep(path);

            }

            if (!reader.Exists(pair.ParentPath))
            {
                return new NonExistentPathAtRoot(path);
            }

            return new PartialFileOrDir(path);
        }


    }
}

public interface IExists;

public interface IFullPath
{
    string FullPath { get; }
}

public interface IParentPath : IFullPath
{
    string ParentPath => Path.GetDirectoryName(FullPath)!;
}

public interface IFileName : IParentPath
{
    string Name => Path.GetFileName(FullPath);
}

public interface IGetChildren;

public interface IRootPath : IFullPath, IExists;

internal record struct ChildDirWithSep(string FullPath) : IParentPath, IGetChildren;

internal record struct ChildDirWithoutSep(string FullPath) : IFileName;

internal record struct PartialFileOrDir(string FullPath) : IFileName;

internal record struct RootPathEndingInColon(string FullPath) : IRootPath;

internal record struct RootPathNotEndingInColon(string FullPath) : IRootPath,IGetChildren;

internal record struct NonExistentPathAtRoot(string FullPath) : IFullPath;

internal record struct NonExistentRootOrDirWithSep(string FullPath) : IFullPath;
