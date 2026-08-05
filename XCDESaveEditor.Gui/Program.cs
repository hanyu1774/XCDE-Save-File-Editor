using System.Drawing;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Photino.NET;
using XCDESaveEditor.Gui.Flows;
using XCDESaveEditor.Gui.Models;
using XCDESaveEditor.Gui.Workflows;

namespace XCDESaveEditor.Gui;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ServiceCollection services = new();

        services.AddScoped<EditorSession>();
        services.AddScoped<LoadFileWorkflow>();
        services.AddScoped<SaveFileWorkflow>();
        services.AddScoped<GetSnapshotWorkflow>();
        services.AddScoped<SetMoneyWorkflow>();
        services.AddScoped<SetNoponstonesWorkflow>();
        services.AddScoped<AddCharacterWorkflow>();
        services.AddScoped<RemoveCharacterWorkflow>();
        services.AddScoped<SetCharacterLevelWorkflow>();
        services.AddScoped<SetCharacterApWorkflow>();
        services.AddScoped<SetCharacterArtSlotWorkflow>();
        services.AddScoped<SetCharacterUnknownBlockWorkflow>();
        services.AddScoped<AddOrUpdateEquipWorkflow>();
        services.AddScoped<RemoveEquipWorkflow>();
        services.AddScoped<AddOrUpdateGemWorkflow>();
        services.AddScoped<RemoveGemWorkflow>();
        services.AddScoped<AddOrUpdateItemWorkflow>();
        services.AddScoped<SetItemQuantityWorkflow>();
        services.AddScoped<RemoveItemWorkflow>();

        ServiceProvider serviceProvider = services.BuildServiceProvider();

        // One long-lived scope for the whole editing session, so the
        // scoped EditorSession keeps the loaded save data between messages.
        IServiceScope sessionScope = serviceProvider.CreateScope();

        PhotinoWindow window = new PhotinoWindow()
            .SetTitle("Xenoblade Chronicles: Definitive Edition - Save Editor")
            .SetUseOsDefaultSize(false)
            .SetSize(new Size(1440, 900))
            .Center()
            .SetResizable(true)
            .SetDevToolsEnabled(true)
            .SetContextMenuEnabled(true)
            .RegisterWebMessageReceivedHandler((object? sender, string message) =>
            {
                PhotinoWindow senderWindow = (PhotinoWindow)sender!;
                string responseJson = HandleIncomingMessage(sessionScope, message, senderWindow);
                senderWindow.SendWebMessage(responseJson);
            })
            .Load("wwwroot/index.html");

        window.WaitForClose();
    }

    private static string HandleIncomingMessage(IServiceScope scope, string rawMessage, PhotinoWindow window)
    {
        ParseClientRequest parseClientRequest = new();
        SerializeClientResponse serializeClientResponse = new();

        ClientResponse response;

        try
        {
            ClientRequest request = parseClientRequest.Run(rawMessage);
            EditorSession editorSession = scope.ServiceProvider.GetRequiredService<EditorSession>();

            response = Dispatch(scope, request, editorSession, window);
        }
        catch (Exception exception)
        {
            response = new ClientResponse { Success = false, Message = "Unexpected error: " + exception.Message };
        }

        return serializeClientResponse.Run(response);
    }

    private static ClientResponse Dispatch(IServiceScope scope, ClientRequest request, EditorSession editorSession, PhotinoWindow window)
    {
        IServiceProvider provider = scope.ServiceProvider;

        return request.Action switch
        {
            "pickAndLoadFile" => HandlePickAndLoadFile(window, provider, editorSession),
            "pickSaveAsLocation" => HandlePickSaveAsLocation(window, provider, editorSession),
            "loadFile" => provider.GetRequiredService<LoadFileWorkflow>().Run(editorSession, request),
            "saveFile" => provider.GetRequiredService<SaveFileWorkflow>().Run(editorSession),
            "getSnapshot" => provider.GetRequiredService<GetSnapshotWorkflow>().Run(editorSession),
            "setMoney" => provider.GetRequiredService<SetMoneyWorkflow>().Run(editorSession, request),
            "setNoponstones" => provider.GetRequiredService<SetNoponstonesWorkflow>().Run(editorSession, request),
            "addCharacter" => provider.GetRequiredService<AddCharacterWorkflow>().Run(editorSession, request),
            "removeCharacter" => provider.GetRequiredService<RemoveCharacterWorkflow>().Run(editorSession, request),
            "setCharacterLevel" => provider.GetRequiredService<SetCharacterLevelWorkflow>().Run(editorSession, request),
            "setCharacterAp" => provider.GetRequiredService<SetCharacterApWorkflow>().Run(editorSession, request),
            "setCharacterArtSlot" => provider.GetRequiredService<SetCharacterArtSlotWorkflow>().Run(editorSession, request),
            "setCharacterUnknownBlock" => provider.GetRequiredService<SetCharacterUnknownBlockWorkflow>().Run(editorSession, request),
            "addOrUpdateEquip" => provider.GetRequiredService<AddOrUpdateEquipWorkflow>().Run(editorSession, request),
            "removeEquip" => provider.GetRequiredService<RemoveEquipWorkflow>().Run(editorSession, request),
            "addOrUpdateGem" => provider.GetRequiredService<AddOrUpdateGemWorkflow>().Run(editorSession, request),
            "removeGem" => provider.GetRequiredService<RemoveGemWorkflow>().Run(editorSession, request),
            "addOrUpdateItem" => provider.GetRequiredService<AddOrUpdateItemWorkflow>().Run(editorSession, request),
            "setItemQuantity" => provider.GetRequiredService<SetItemQuantityWorkflow>().Run(editorSession, request),
            "removeItem" => provider.GetRequiredService<RemoveItemWorkflow>().Run(editorSession, request),
            _ => new ClientResponse { Success = false, Message = "Unknown action: " + request.Action },
        };
    }

    private static ClientResponse HandlePickAndLoadFile(PhotinoWindow window, IServiceProvider provider, EditorSession editorSession)
    {
        string[] selectedFiles = window.ShowOpenFile(
            "Select a Xenoblade Chronicles: Definitive Edition save file",
            "",
            false,
            new (string, string[])[] { ("Save files", new[] { "*.sav" }), ("All files", new[] { "*.*" }) });

        if (selectedFiles.Length == 0)
        {
            return new ClientResponse { Success = false, Message = "No file selected." };
        }

        ClientRequest syntheticRequest = BuildRequestWithFilePath("loadFile", selectedFiles[0]);
        return provider.GetRequiredService<LoadFileWorkflow>().Run(editorSession, syntheticRequest);
    }

    private static ClientResponse HandlePickSaveAsLocation(PhotinoWindow window, IServiceProvider provider, EditorSession editorSession)
    {
        if (!editorSession.IsLoaded || editorSession.SaveData is null)
        {
            return new ClientResponse { Success = false, Message = "No save file is currently loaded." };
        }

        string selectedPath = window.ShowSaveFile(
            "Save as",
            editorSession.FilePath,
            new (string, string[])[] { ("Save files", new[] { "*.sav" }) });

        if (string.IsNullOrWhiteSpace(selectedPath))
        {
            return new ClientResponse { Success = false, Message = "No location selected." };
        }

        editorSession.FilePath = selectedPath;
        return provider.GetRequiredService<SaveFileWorkflow>().Run(editorSession);
    }

    private static ClientRequest BuildRequestWithFilePath(string action, string filePath)
    {
        string json = JsonSerializer.Serialize(new { filePath });
        JsonDocument document = JsonDocument.Parse(json);
        return new ClientRequest { Action = action, Data = document.RootElement };
    }
}
