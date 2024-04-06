﻿using CommandLine;

namespace Lokql.Engine;

public static class StandardParsers
{
    public static readonly Parser Default = new(settings =>
                                                  {
                                                      settings.CaseInsensitiveEnumValues = true;
                                                      settings.CaseSensitive = false;
                                                      settings.HelpWriter = Console.Out;
                                                  });

    public static Parser CreateWithHelpWriter(TextWriter writer)
    =>  new(settings =>
    {
        settings.CaseInsensitiveEnumValues = true;
        settings.CaseSensitive = false;
        settings.HelpWriter =writer;
    });
}


public static class ParserExtensions
{
    public static async Task<ParserResult<object>> WithParsedAsync<T>(this Task<ParserResult<object>> resultTask,
        Func<T, Task> action)
    {
        var result = await resultTask;
        if (result is Parsed<object> parsed && parsed.Value is T value)
        {
            await action(value);
        }

        return result;
    }
}
