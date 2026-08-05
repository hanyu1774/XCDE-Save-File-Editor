using XCDESave;
using XCDESaveEditor.Gui.Flows;
using XCDESaveEditor.Gui.Models;

namespace XCDESaveEditor.Gui.Workflows;

public sealed class AddOrUpdateEquipWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseAddOrUpdateEquipRequest parseAddOrUpdateEquipRequest = new();
        ResolveEquipBox resolveEquipBox = new();
        FindFirstFreeEquipSlot findFirstFreeEquipSlot = new();
        GetNextEquipSerialNumber getNextEquipSerialNumber = new();
        AddOrUpdateEquipItem addOrUpdateEquipItem = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        AddOrUpdateEquipPayload payload = parseAddOrUpdateEquipRequest.Run(request);
        EquipBoxResolution boxResolution = resolveEquipBox.Run(saveData, payload.Category);

        if (!boxResolution.Success)
        {
            return buildErrorResponse.Run("Unknown equipment category: " + payload.Category);
        }

        int slotIndex = payload.SlotIndex ?? findFirstFreeEquipSlot.Run(boxResolution.Box);

        if (slotIndex == -1)
        {
            return buildErrorResponse.Run("No free slot available (500/500 used).");
        }

        if (slotIndex < 0 || slotIndex >= boxResolution.Box.Length)
        {
            return buildErrorResponse.Run("Slot index must be between 0 and 499.");
        }

        uint nextSerial = getNextEquipSerialNumber.Run(boxResolution.Box);

        addOrUpdateEquipItem.Run(
            boxResolution.Box, slotIndex, payload.ItemId, boxResolution.ItemType, payload.Quantity,
            payload.GemSlots, payload.Gem1Id, payload.Gem2Id, payload.Gem3Id, nextSerial);

        return buildSuccessResponse.Run(saveData, $"Item placed in slot {slotIndex}.");
    }
}

public sealed class RemoveEquipWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseCategoryAndSlotRequest parseCategoryAndSlotRequest = new();
        ResolveEquipBox resolveEquipBox = new();
        RemoveEquipItem removeEquipItem = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        CategoryAndSlotPayload payload = parseCategoryAndSlotRequest.Run(request);
        EquipBoxResolution boxResolution = resolveEquipBox.Run(saveData, payload.Category);

        if (!boxResolution.Success)
        {
            return buildErrorResponse.Run("Unknown equipment category: " + payload.Category);
        }

        if (payload.SlotIndex < 0 || payload.SlotIndex >= boxResolution.Box.Length)
        {
            return buildErrorResponse.Run("Slot index must be between 0 and 499.");
        }

        removeEquipItem.Run(boxResolution.Box, payload.SlotIndex);

        return buildSuccessResponse.Run(saveData, $"Slot {payload.SlotIndex} cleared.");
    }
}
