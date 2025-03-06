using System.IO.Abstractions;

namespace KustoLoco.Core.Intellisense;

public static class IntellisenseServiceProvider
{
    private static readonly FileSystem FileSystem = new();
    public static IIntellisenseService GetIntellisenseService()
    {
        return new IntellisenseServiceWithParser(new IntellisenseService(FileSystem));
    }
}
