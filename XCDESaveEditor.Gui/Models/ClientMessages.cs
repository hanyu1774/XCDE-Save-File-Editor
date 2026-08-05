using System.Text.Json;

namespace XCDESaveEditor.Gui.Models;

public sealed class ClientRequest
{
    public string Action = string.Empty;
    public JsonElement Data;
}

public sealed class ClientResponse
{
    public bool Success;
    public string Message = string.Empty;
    public string? FilePath;
    public object? Data;
}
