using System.Collections.Immutable;

namespace Intellisense;

public record CompletionResult
{
    public static readonly CompletionResult Empty = new();
    /// <summary>
    /// List of available completion entries.
    /// </summary>
    public IList<IntellisenseEntry> Entries { get; init; } = ImmutableArray<IntellisenseEntry>.Empty;
    /// <summary>
    /// The text filter to be applied to the completion entries.
    /// </summary>
    public string Filter { get; init; } = string.Empty;
}

