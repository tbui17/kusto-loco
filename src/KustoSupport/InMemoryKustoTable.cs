﻿using System.Collections;
using System.Collections.Specialized;
using BabyKusto.Core;
using BabyKusto.Core.Util;
using Kusto.Language.Symbols;
using NLog;

#pragma warning disable CS8604 // Possible null reference argument.

#pragma warning disable CS8602 // Dereference of a possibly null reference.

#pragma warning disable CS8603 // Possible null reference return.

namespace KustoSupport;

/// <summary>
///     Provides a simple way to create a Kusto ITableSource
/// </summary>
/// <remarks>
///     TODO This is currently a bit incomplete - more type support is needed
/// </remarks>
public class InMemoryKustoTable : BaseKustoTable
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    private InMemoryKustoTable(TableSymbol tableSym, IEnumerable<BaseColumnBuilder> builders, int length) : base(
        tableSym,
        length)
    {
        Builders = builders.ToArray();
        Length = length;
    }

    public BaseColumnBuilder[] Builders { get; }


    public override IEnumerable<ITableChunk> GetData()
    {
        yield return new TableChunk(this, Builders.Select(b => b.ToColumn()).ToArray());
    }


    public override IAsyncEnumerable<ITableChunk> GetDataAsync(CancellationToken cancellation = default)
        => throw new NotSupportedException();

    /// <summary>
    ///     Shares the data in a table under a different name
    /// </summary>
    public InMemoryKustoTable ShareAs(string newName)
    {
        var newTableSymbol = new TableSymbol(newName, Type.Columns, Type.Description);
        return new InMemoryKustoTable(newTableSymbol, Builders, Length);
    }


    public static InMemoryKustoTable FromDefinition(KustoTableDefinition definition)
        => new(definition.Symbol, definition.Columns, definition.RowCount);

    public static InMemoryKustoTable CreateFromRows<T>(string name, IReadOnlyCollection<T> rows)
    {
        var tableDefinition = FromRecords(name, rows);
        return FromDefinition(tableDefinition);
    }

    private static KustoTableDefinition FromRecords<T>(string tableName, IReadOnlyCollection<T> records)
    {
        var columnDefinitions = typeof(T).GetProperties()
            .Select(p => p.PropertyType switch
            {
                _ when p.PropertyType == typeof(int) =>
                    new KustoColumnDefinition<T>(p.Name, ScalarTypes.Int,
                        x => p.GetValue(x)),
                _ when p.PropertyType == typeof(DateTime) =>
                    new KustoColumnDefinition<T>(p.Name, ScalarTypes.DateTime,
                        x => p.GetValue(x)),
                _ when p.PropertyType == typeof(double) =>
                    new KustoColumnDefinition<T>(p.Name, ScalarTypes.Real,
                        x => p.GetValue(x)),
                _ => new KustoColumnDefinition<T>(p.Name, ScalarTypes.String,
                    x => p.GetValue(x).ToString()),
            })
            .ToList();

        return FromRows(tableName, records, columnDefinitions);
    }

    public static KustoTableDefinition FromRows<T>(string tableName,
        IReadOnlyCollection<T> rows,
        IReadOnlyCollection<KustoColumnDefinition<T>> columnDefinitions)
    {
        var tableSymbol =
            new TableSymbol(tableName, columnDefinitions
                .Select(p => new ColumnSymbol(p.Name, p.Type))
                .ToArray());

        var allBuilders = columnDefinitions
            .Select(Create)
            .ToArray();

        return new KustoTableDefinition(tableSymbol, allBuilders, rows.Count);

        BaseColumnBuilder Create(KustoColumnDefinition<T> c)
        {
            var builder = ColumnHelpers.CreateBuilder(c.Type);
            foreach (var r in rows)
            {
                builder.Add(c.Value(r));
            }

            return builder;
        }
    }

    public static InMemoryKustoTable FromOrderedDictionarySet(string tableName,
        IReadOnlyCollection<OrderedDictionary> dictionaries)
    {
        //currently we rely on the dictionary having valid types and avoiding null values in unfortunate places
        var firstDictionary = dictionaries.First();
        var headers = firstDictionary.Cast<DictionaryEntry>()
            .Select(de => de.Key.ToString())
            .Select(h => new { ColumnName = h, Type = firstDictionary[h].GetType() })
            .ToArray();
        var columnDefinitions = headers
            .Select(h => h switch
            {
                _ when h.Type == typeof(int) =>
                    new KustoColumnDefinition<int>(h.ColumnName, ScalarTypes.Int,
                        x => dictionaries.ElementAt(x)[h.ColumnName]),
                _ when h.Type == typeof(DateTime) =>
                    new KustoColumnDefinition<int>(h.ColumnName, ScalarTypes.DateTime,
                        x => dictionaries.ElementAt(x)[h.ColumnName]),
                _ when h.Type == typeof(double) =>
                    new KustoColumnDefinition<int>(h.ColumnName, ScalarTypes.Real,
                        x => dictionaries.ElementAt(x)[h.ColumnName]),
                _ => new KustoColumnDefinition<int>(h.ColumnName, ScalarTypes.String,
                    x => dictionaries.ElementAt(x)[h.ColumnName].ToString()),
            })
            .ToList();

        var definition = FromRows(tableName,
            Enumerable.Range(0, dictionaries.Count).ToArray(),
            columnDefinitions);
        return FromDefinition(definition);
    }
}