using System.Collections.Immutable;

namespace Intellisense;

public abstract record CompletionResult
{
    public static readonly CompletionResult Empty = new EmptyCompletionResult();
    public IEnumerable<IntellisenseEntry> Entries { get; init; } = ImmutableArray<IntellisenseEntry>.Empty;
    public string Prefix { get; init; } = string.Empty;
    public int Rewind { get; init; }
}

public record BasicCompletionResult : CompletionResult;

public record FilterCompletionResult : CompletionResult
{
    public required string Filter { get; init; }
}

public record EmptyCompletionResult : CompletionResult;
