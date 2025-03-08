using System.Collections.Immutable;

namespace Intellisense;

public record CompletionResult
{
    public static readonly CompletionResult Empty = new();

    public IEnumerable<IntellisenseEntry> Entries = ImmutableArray<IntellisenseEntry>.Empty;
    public string Prefix = string.Empty;
    public int Rewind;
}
