using XCDESave;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class GetNextItemSerialNumber
{
    public uint Run(Item[] box)
    {
        uint highest = 0;

        foreach (Item item in box)
        {
            if (item.Exists && item.SerialNo > highest)
            {
                highest = item.SerialNo;
            }
        }

        return highest + 1;
    }
}

internal sealed class FindFirstFreeItemSlot
{
    public int Run(Item[] box)
    {
        for (int i = 0; i < box.Length; i++)
        {
            if (!box[i].Exists)
            {
                return i;
            }
        }

        return -1;
    }
}

internal sealed class AddOrUpdateItem
{
    public void Run(Item[] box, int slotIndex, ushort itemId, ItemType itemType, ushort quantity, uint nextSerialNumber)
    {
        Item item = box[slotIndex];

        item.Exists = true;
        item.Index = (ushort)slotIndex;
        item.Type = itemType;
        item.FullID = new ItemID { ID = itemId, TypeID = itemType };
        item.Quantity = quantity;

        if (item.SerialNo == 0)
        {
            item.SerialNo = nextSerialNumber;
        }
    }
}

internal sealed class SetItemQuantity
{
    public void Run(Item[] box, int slotIndex, ushort newQuantity)
    {
        box[slotIndex].Quantity = newQuantity;
    }
}

internal sealed class RemoveItem
{
    public void Run(Item[] box, int slotIndex)
    {
        Item item = box[slotIndex];

        item.Exists = false;
        item.Index = 0;
        item.Type = ItemType.None;
        item.FullID = new ItemID();
        item.Quantity = 0;
        item.SerialNo = 0;
    }
}
