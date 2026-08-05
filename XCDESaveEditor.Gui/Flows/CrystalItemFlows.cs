using XCDESave;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class GetNextCrystalSerialNumber
{
    public uint Run(CrystalItem[] box)
    {
        uint highest = 0;

        foreach (CrystalItem item in box)
        {
            if (item.Exists && item.SerialNo > highest)
            {
                highest = item.SerialNo;
            }
        }

        return highest + 1;
    }
}

internal sealed class FindFirstFreeCrystalSlot
{
    public int Run(CrystalItem[] box)
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

internal sealed class AddOrUpdateCrystalItem
{
    public void Run(
        CrystalItem[] box,
        int slotIndex,
        ushort itemId,
        ItemType itemType,
        ushort quantity,
        ushort crystalNameId,
        byte rank,
        byte element,
        ushort buff1Id,
        ushort buff1Value,
        ushort buff2Id,
        ushort buff2Value,
        ushort buff3Id,
        ushort buff3Value,
        ushort buff4Id,
        ushort buff4Value,
        uint nextSerialNumber)
    {
        CrystalItem item = box[slotIndex];

        item.Exists = true;
        item.Index = (ushort)slotIndex;
        item.Type = itemType;
        item.FullID = new ItemID { ID = itemId, TypeID = itemType };
        item.Quantity = quantity;

        if (item.SerialNo == 0)
        {
            item.SerialNo = nextSerialNumber;
        }

        item.CrystalNameID = crystalNameId;
        item.Rank = rank;
        item.Element = element;
        item.BuffCount = CountActiveBuffs(buff1Id, buff2Id, buff3Id, buff4Id);
        item.Buff1ID = buff1Id;
        item.Buff1Value = buff1Value;
        item.Buff2ID = buff2Id;
        item.Buff2Value = buff2Value;
        item.Buff3ID = buff3Id;
        item.Buff3Value = buff3Value;
        item.Buff4ID = buff4Id;
        item.Buff4Value = buff4Value;
    }

    private static ushort CountActiveBuffs(ushort buff1Id, ushort buff2Id, ushort buff3Id, ushort buff4Id)
    {
        ushort count = 0;

        if (buff1Id != 0) count++;
        if (buff2Id != 0) count++;
        if (buff3Id != 0) count++;
        if (buff4Id != 0) count++;

        return count;
    }
}

internal sealed class RemoveCrystalItem
{
    public void Run(CrystalItem[] box, int slotIndex)
    {
        CrystalItem item = box[slotIndex];

        item.Exists = false;
        item.Index = 0;
        item.Type = ItemType.None;
        item.FullID = new ItemID();
        item.Quantity = 0;
        item.SerialNo = 0;
        item.CrystalNameID = 0;
        item.Rank = 0;
        item.Element = 0;
        item.BuffCount = 0;
        item.Buff1ID = 0;
        item.Buff1Value = 0;
        item.Buff2ID = 0;
        item.Buff2Value = 0;
        item.Buff3ID = 0;
        item.Buff3Value = 0;
        item.Buff4ID = 0;
        item.Buff4Value = 0;
    }
}
