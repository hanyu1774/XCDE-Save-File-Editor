using XCDESave;
using XCDESaveEditor.Gui.Flows;
using XCDESaveEditor.Gui.Models;

namespace XCDESaveEditor.Gui.Workflows;

public sealed class AddOrUpdateGemWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseAddOrUpdateGemRequest parseAddOrUpdateGemRequest = new();
        ResolveCrystalBox resolveCrystalBox = new();
        FindFirstFreeCrystalSlot findFirstFreeCrystalSlot = new();
        GetNextCrystalSerialNumber getNextCrystalSerialNumber = new();
        AddOrUpdateCrystalItem addOrUpdateCrystalItem = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        AddOrUpdateGemPayload payload = parseAddOrUpdateGemRequest.Run(request);
        CrystalBoxResolution boxResolution = resolveCrystalBox.Run(saveData, payload.Category);

        if (!boxResolution.Success)
        {
            return buildErrorResponse.Run("Unknown gemstone category: " + payload.Category);
        }

        int slotIndex = payload.SlotIndex ?? findFirstFreeCrystalSlot.Run(boxResolution.Box);

        if (slotIndex == -1)
        {
            return buildErrorResponse.Run("No free slot available (500/500 used).");
        }

        if (slotIndex < 0 || slotIndex >= boxResolution.Box.Length)
        {
            return buildErrorResponse.Run("Slot index must be between 0 and 499.");
        }

        uint nextSerial = getNextCrystalSerialNumber.Run(boxResolution.Box);

        addOrUpdateCrystalItem.Run(
            boxResolution.Box, slotIndex, payload.ItemId, boxResolution.ItemType, payload.Quantity,
            payload.CrystalNameId, payload.Rank, payload.Element,
            payload.Buff1Id, payload.Buff1Value, payload.Buff2Id, payload.Buff2Value,
            payload.Buff3Id, payload.Buff3Value, payload.Buff4Id, payload.Buff4Value, nextSerial);

        return buildSuccessResponse.Run(saveData, $"Item placed in slot {slotIndex}.");
    }
}

public sealed class RemoveGemWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseCategoryAndSlotRequest parseCategoryAndSlotRequest = new();
        ResolveCrystalBox resolveCrystalBox = new();
        RemoveCrystalItem removeCrystalItem = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        CategoryAndSlotPayload payload = parseCategoryAndSlotRequest.Run(request);
        CrystalBoxResolution boxResolution = resolveCrystalBox.Run(saveData, payload.Category);

        if (!boxResolution.Success)
        {
            return buildErrorResponse.Run("Unknown gemstone category: " + payload.Category);
        }

        if (payload.SlotIndex < 0 || payload.SlotIndex >= boxResolution.Box.Length)
        {
            return buildErrorResponse.Run("Slot index must be between 0 and 499.");
        }

        removeCrystalItem.Run(boxResolution.Box, payload.SlotIndex);

        return buildSuccessResponse.Run(saveData, $"Slot {payload.SlotIndex} cleared.");
    }
}
