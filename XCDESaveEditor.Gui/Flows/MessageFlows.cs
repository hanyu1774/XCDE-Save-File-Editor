using System.Text.Json;
using XCDESaveEditor.Gui.Models;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class ParseClientRequest
{
    public ClientRequest Run(string rawMessage)
    {
        JsonDocument document = JsonDocument.Parse(rawMessage);
        JsonElement root = document.RootElement;

        string action = root.TryGetProperty("action", out JsonElement actionElement)
            ? actionElement.GetString() ?? string.Empty
            : string.Empty;

        JsonElement data = root.TryGetProperty("data", out JsonElement dataElement)
            ? dataElement
            : default;

        return new ClientRequest { Action = action, Data = data };
    }
}

internal sealed class SerializeClientResponse
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IncludeFields = true,
    };

    public string Run(ClientResponse response)
    {
        return JsonSerializer.Serialize(response, SerializerOptions);
    }
}
