using XCDESave;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class GetNextEquipSerialNumber
{
    public uint Run(EquipItem[] box)
    {
        uint highest = 0;

        foreach (EquipItem item in box)
        {
            if (item.Exists && item.SerialNo > highest)
            {
                highest = item.SerialNo;
            }
        }

        return highest + 1;
    }
}

internal sealed class FindFirstFreeEquipSlot
{
    public int Run(EquipItem[] box)
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

internal sealed class AddOrUpdateEquipItem
{
    public void Run(
        EquipItem[] box,
        int slotIndex,
        ushort itemId,
        ItemType itemType,
        ushort quantity,
        byte gemSlots,
        ushort gem1Id,
        ushort gem2Id,
        ushort gem3Id,
        uint nextSerialNumber)
    {
        EquipItem item = box[slotIndex];

        item.Exists = true;
        item.Index = (ushort)slotIndex;
        item.Type = itemType;
        item.FullID = new ItemID { ID = itemId, TypeID = itemType };
        item.Quantity = quantity;

        if (item.SerialNo == 0)
        {
            item.SerialNo = nextSerialNumber;
        }

        byte clampedGemSlots = gemSlots > 3 ? (byte)3 : gemSlots;
        item.GemSlots = clampedGemSlots;

        item.Gem1 = BuildGemId(gem1Id);
        item.Gem2 = BuildGemId(gem2Id);
        item.Gem3 = BuildGemId(gem3Id);
    }

    private static ItemID BuildGemId(ushort gemId)
    {
        if (gemId == 0)
        {
            return new ItemID();
        }

        return new ItemID { ID = gemId, TypeID = ItemType.Gem };
    }
}

internal sealed class RemoveEquipItem
{
    public void Run(EquipItem[] box, int slotIndex)
    {
        EquipItem item = box[slotIndex];

        // Zeroing Index, Type and SerialNo as well as Exists/FullID mirrors what
        // the game itself does when it clears a slot (observed by diffing two
        // real save files where the game removed a gem from an equipped slot).
        item.Exists = false;
        item.Index = 0;
        item.Type = ItemType.None;
        item.FullID = new ItemID();
        item.Quantity = 0;
        item.SerialNo = 0;
        item.GemSlots = 0;
        item.Gem1 = new ItemID();
        item.Gem2 = new ItemID();
        item.Gem3 = new ItemID();
    }
}
