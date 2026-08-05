using XCDESave;
using XCDESaveEditor.Gui.Models;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class BuildSuccessResponse
{
    public ClientResponse Run(XCDESaveData saveData, string message)
    {
        BuildSaveSnapshot buildSaveSnapshot = new();
        SaveSnapshot snapshot = buildSaveSnapshot.Run(saveData);

        return new ClientResponse { Success = true, Message = message, Data = snapshot };
    }
}

internal sealed class BuildErrorResponse
{
    public ClientResponse Run(string message)
    {
        return new ClientResponse { Success = false, Message = message };
    }
}
