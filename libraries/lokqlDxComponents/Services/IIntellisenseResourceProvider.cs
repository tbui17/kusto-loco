using Intellisense;

namespace lokqlDxComponents.Services;

public interface IIntellisenseResourceProvider
{
    IntellisenseEntry[] InternalCommands { get; }
    IntellisenseEntry[] KqlFunctionEntries { get; }
    IntellisenseEntry[] SettingNames { get; }
    IntellisenseEntry[] KqlOperatorEntries { get; }
    IntellisenseEntry[] GetTables(string blockText);
    IntellisenseEntry[] GetColumns(string blockText);
}
