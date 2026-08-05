namespace XCDESaveEditor.Gui.Models;

public sealed class AddOrUpdateEquipPayload
{
    public string Category = string.Empty;
    public int? SlotIndex;
    public ushort ItemId;
    public ushort Quantity;
    public byte GemSlots;
    public ushort Gem1Id;
    public ushort Gem2Id;
    public ushort Gem3Id;
}

public sealed class AddOrUpdateGemPayload
{
    public string Category = string.Empty;
    public int? SlotIndex;
    public ushort ItemId;
    public ushort Quantity;
    public ushort CrystalNameId;
    public byte Rank;
    public byte Element;
    public ushort Buff1Id;
    public ushort Buff1Value;
    public ushort Buff2Id;
    public ushort Buff2Value;
    public ushort Buff3Id;
    public ushort Buff3Value;
    public ushort Buff4Id;
    public ushort Buff4Value;
}

public sealed class AddOrUpdateItemPayload
{
    public string Category = string.Empty;
    public int? SlotIndex;
    public ushort ItemId;
    public ushort Quantity;
}

public sealed class CategoryAndSlotPayload
{
    public string Category = string.Empty;
    public int SlotIndex;
}

public sealed class SetItemQuantityPayload
{
    public string Category = string.Empty;
    public int SlotIndex;
    public ushort Quantity;
}

public sealed class SetCharacterStatPayload
{
    public int CharacterId;
    public uint Value;
}

public sealed class SetArtSlotPayload
{
    public int CharacterId;
    public int SlotIndex;
    public ushort ArtId;
    public bool UseMonadoSet;
}

public sealed class SetUnknownBlockPayload
{
    public int CharacterId;
    public string BlockName = string.Empty;
    public string HexValue = string.Empty;
}

public sealed class HexParseResult
{
    public bool Success;
    public string ErrorMessage = string.Empty;
    public byte[] Bytes = System.Array.Empty<byte>();
}
