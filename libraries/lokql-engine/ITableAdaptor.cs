﻿using KustoLoco.Core;
namespace Lokql.Engine;

public interface ITableAdaptor : IKustoQueryContextTableLoader
{
    Task<bool> LoadTable(KustoQueryContext context, string path, string tableName,IProgress<string> progressReporter);
    Task<bool> SaveResult(KustoQueryResult result, string path,IProgress<string> progressReporter);
    IEnumerable<TableAdaptorDescription> GetSupportedAdaptors();

}
