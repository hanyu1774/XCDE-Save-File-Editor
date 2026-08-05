using XCDESave;
using XCDESaveEditor.Gui.Flows;
using XCDESaveEditor.Gui.Models;

namespace XCDESaveEditor.Gui.Workflows;

public sealed class AddOrUpdateItemWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseAddOrUpdateItemRequest parseAddOrUpdateItemRequest = new();
        ResolveItemBox resolveItemBox = new();
        FindFirstFreeItemSlot findFirstFreeItemSlot = new();
        GetNextItemSerialNumber getNextItemSerialNumber = new();
        AddOrUpdateItem addOrUpdateItem = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        AddOrUpdateItemPayload payload = parseAddOrUpdateItemRequest.Run(request);
        ItemBoxResolution boxResolution = resolveItemBox.Run(saveData, payload.Category);

        if (!boxResolution.Success)
        {
            return buildErrorResponse.Run("Unknown item category: " + payload.Category);
        }

        int slotIndex = payload.SlotIndex ?? findFirstFreeItemSlot.Run(boxResolution.Box);

        if (slotIndex == -1)
        {
            return buildErrorResponse.Run("No free slot available (500/500 used).");
        }

        if (slotIndex < 0 || slotIndex >= boxResolution.Box.Length)
        {
            return buildErrorResponse.Run("Slot index must be between 0 and 499.");
        }

        uint nextSerial = getNextItemSerialNumber.Run(boxResolution.Box);

        addOrUpdateItem.Run(boxResolution.Box, slotIndex, payload.ItemId, boxResolution.ItemType, payload.Quantity, nextSerial);

        return buildSuccessResponse.Run(saveData, $"Item placed in slot {slotIndex}.");
    }
}

public sealed class SetItemQuantityWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseSetItemQuantityRequest parseSetItemQuantityRequest = new();
        ResolveItemBox resolveItemBox = new();
        SetItemQuantity setItemQuantity = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        SetItemQuantityPayload payload = parseSetItemQuantityRequest.Run(request);
        ItemBoxResolution boxResolution = resolveItemBox.Run(saveData, payload.Category);

        if (!boxResolution.Success)
        {
            return buildErrorResponse.Run("Unknown item category: " + payload.Category);
        }

        if (payload.SlotIndex < 0 || payload.SlotIndex >= boxResolution.Box.Length)
        {
            return buildErrorResponse.Run("Slot index must be between 0 and 499.");
        }

        setItemQuantity.Run(boxResolution.Box, payload.SlotIndex, payload.Quantity);

        return buildSuccessResponse.Run(saveData, $"Quantity in slot {payload.SlotIndex} updated.");
    }
}

public sealed class RemoveItemWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseCategoryAndSlotRequest parseCategoryAndSlotRequest = new();
        ResolveItemBox resolveItemBox = new();
        RemoveItem removeItem = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        CategoryAndSlotPayload payload = parseCategoryAndSlotRequest.Run(request);
        ItemBoxResolution boxResolution = resolveItemBox.Run(saveData, payload.Category);

        if (!boxResolution.Success)
        {
            return buildErrorResponse.Run("Unknown item category: " + payload.Category);
        }

        if (payload.SlotIndex < 0 || payload.SlotIndex >= boxResolution.Box.Length)
        {
            return buildErrorResponse.Run("Slot index must be between 0 and 499.");
        }

        removeItem.Run(boxResolution.Box, payload.SlotIndex);

        return buildSuccessResponse.Run(saveData, $"Slot {payload.SlotIndex} cleared.");
    }
}
