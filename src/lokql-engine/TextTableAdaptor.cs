﻿using KustoSupport;

public class TextTableAdaptor : IFileBasedTableAccess
{
    public IReadOnlyCollection<string> SupportedFileExtensions() => [".txt"];

    public Task TrySave(string path, KustoQueryResult result)
    {
        var text = KustoFormatter.Tabulate(result);
        File.WriteAllText(path, text);
        return Task.CompletedTask;
    }

    public Task<bool> TryLoad(string path, KustoQueryContext context, string name)
    {
        var lines = File.ReadAllLines(path)
            .Select(l => new { Line = l })
            .ToArray();
        var table = TableBuilder.CreateFromRows(name, lines).ToTableSource();
        context.AddTable(table);
        return Task.FromResult(true);
    }
}