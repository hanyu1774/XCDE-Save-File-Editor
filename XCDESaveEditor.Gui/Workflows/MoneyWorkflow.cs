using XCDESave;
using XCDESaveEditor.Gui.Flows;
using XCDESaveEditor.Gui.Models;

namespace XCDESaveEditor.Gui.Workflows;

public sealed class SetMoneyWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseMoneyRequest parseMoneyRequest = new();
        SetMoney setMoney = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        uint newAmount = parseMoneyRequest.Run(request);

        setMoney.Run(saveData, newAmount);

        return buildSuccessResponse.Run(saveData, "Money updated.");
    }
}

public sealed class SetNoponstonesWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseNoponstonesRequest parseNoponstonesRequest = new();
        SetNoponstones setNoponstones = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        uint newAmount = parseNoponstonesRequest.Run(request);

        setNoponstones.Run(saveData, newAmount);

        return buildSuccessResponse.Run(saveData, "Noponstones updated.");
    }
}
