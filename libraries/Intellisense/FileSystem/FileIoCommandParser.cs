using System.CommandLine.Parsing;

namespace Intellisense.FileSystem;

public class FileIoCommandParser
{
    public RootedPath? ParseRootedPathFromLastArg(string lineText)
    {
        var args = CommandLineStringSplitter.Instance.Split(lineText).ToList();
        if (args.Count < 2)
        {
            return null;
        }

        if (args[0] is not (".save" or ".load"))
        {
            return null;
        }

        return RootedPath.Create(args[^1]);
    }
}
