using System.Reflection;
using System.Text.Json;

namespace ScreenHunter.Presentation.WinForms;

public sealed class L10n
{
    Dictionary<string, string> strings = [];
    public string Language { get; private set; } = "es";
    public void Set(string language)
    {
        Language = language;
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetManifestResourceNames().Single(n => n.EndsWith($".Resources.{language}.json", StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(name) ?? throw new InvalidDataException("Missing localization");
        strings = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)!;
    }
    public string this[string key] => strings.GetValueOrDefault(key, key);
    public string Tool(EditorTool tool) => this[tool.ToString()];
    public string Error(Exception e)
    {
        if (strings.TryGetValue(e.Message, out var message)) return message;
        return this[e switch { UnauthorizedAccessException => "AccessError", IOException => "FileError", System.Runtime.InteropServices.ExternalException => "WindowsError", _ => "OperationError" }];
    }
}
