namespace Intellisense;

public record CompletionResult
{
    public IEnumerable<IntellisenseEntry> Entries = [];
    public string Prefix = string.Empty;
    public int Rewind;
}
