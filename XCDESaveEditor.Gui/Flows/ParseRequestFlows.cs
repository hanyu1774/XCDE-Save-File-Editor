using System.Text.Json;
using XCDESaveEditor.Gui.Models;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class ParseFilePathRequest
{
    public string Run(ClientRequest request)
    {
        return request.Data.GetProperty("filePath").GetString() ?? string.Empty;
    }
}

internal sealed class ParseMoneyRequest
{
    public uint Run(ClientRequest request)
    {
        return request.Data.GetProperty("amount").GetUInt32();
    }
}

internal sealed class ParseNoponstonesRequest
{
    public uint Run(ClientRequest request)
    {
        return request.Data.GetProperty("amount").GetUInt32();
    }
}

internal sealed class ParseCharacterIdRequest
{
    public int Run(ClientRequest request)
    {
        return request.Data.GetProperty("characterId").GetInt32();
    }
}

internal sealed class ParseSetLevelRequest
{
    public SetCharacterStatPayload Run(ClientRequest request)
    {
        return new SetCharacterStatPayload
        {
            CharacterId = request.Data.GetProperty("characterId").GetInt32(),
            Value = request.Data.GetProperty("level").GetUInt32(),
        };
    }
}

internal sealed class ParseSetApRequest
{
    public SetCharacterStatPayload Run(ClientRequest request)
    {
        return new SetCharacterStatPayload
        {
            CharacterId = request.Data.GetProperty("characterId").GetInt32(),
            Value = request.Data.GetProperty("ap").GetUInt32(),
        };
    }
}

internal sealed class ParseSetArtSlotRequest
{
    public SetArtSlotPayload Run(ClientRequest request)
    {
        bool useMonadoSet = request.Data.TryGetProperty("useMonadoSet", out JsonElement monadoElement) && monadoElement.GetBoolean();

        return new SetArtSlotPayload
        {
            CharacterId = request.Data.GetProperty("characterId").GetInt32(),
            SlotIndex = request.Data.GetProperty("slotIndex").GetInt32(),
            ArtId = request.Data.GetProperty("artId").GetUInt16(),
            UseMonadoSet = useMonadoSet,
        };
    }
}

internal sealed class ParseSetUnknownBlockRequest
{
    public SetUnknownBlockPayload Run(ClientRequest request)
    {
        return new SetUnknownBlockPayload
        {
            CharacterId = request.Data.GetProperty("characterId").GetInt32(),
            BlockName = request.Data.GetProperty("blockName").GetString() ?? string.Empty,
            HexValue = request.Data.GetProperty("hexValue").GetString() ?? string.Empty,
        };
    }
}

internal sealed class ParseCategoryAndSlotRequest
{
    public CategoryAndSlotPayload Run(ClientRequest request)
    {
        return new CategoryAndSlotPayload
        {
            Category = request.Data.GetProperty("category").GetString() ?? string.Empty,
            SlotIndex = request.Data.GetProperty("slotIndex").GetInt32(),
        };
    }
}

internal sealed class ParseSetItemQuantityRequest
{
    public SetItemQuantityPayload Run(ClientRequest request)
    {
        return new SetItemQuantityPayload
        {
            Category = request.Data.GetProperty("category").GetString() ?? string.Empty,
            SlotIndex = request.Data.GetProperty("slotIndex").GetInt32(),
            Quantity = request.Data.GetProperty("quantity").GetUInt16(),
        };
    }
}

internal sealed class ParseAddOrUpdateEquipRequest
{
    public AddOrUpdateEquipPayload Run(ClientRequest request)
    {
        return new AddOrUpdateEquipPayload
        {
            Category = request.Data.GetProperty("category").GetString() ?? string.Empty,
            SlotIndex = request.Data.TryGetProperty("slotIndex", out JsonElement slotElement) ? slotElement.GetInt32() : null,
            ItemId = request.Data.GetProperty("itemId").GetUInt16(),
            Quantity = request.Data.TryGetProperty("quantity", out JsonElement qtyElement) ? qtyElement.GetUInt16() : (ushort)1,
            GemSlots = request.Data.TryGetProperty("gemSlots", out JsonElement gemSlotsElement) ? gemSlotsElement.GetByte() : (byte)0,
            Gem1Id = request.Data.TryGetProperty("gem1Id", out JsonElement gem1Element) ? gem1Element.GetUInt16() : (ushort)0,
            Gem2Id = request.Data.TryGetProperty("gem2Id", out JsonElement gem2Element) ? gem2Element.GetUInt16() : (ushort)0,
            Gem3Id = request.Data.TryGetProperty("gem3Id", out JsonElement gem3Element) ? gem3Element.GetUInt16() : (ushort)0,
        };
    }
}

internal sealed class ParseAddOrUpdateGemRequest
{
    public AddOrUpdateGemPayload Run(ClientRequest request)
    {
        return new AddOrUpdateGemPayload
        {
            Category = request.Data.GetProperty("category").GetString() ?? string.Empty,
            SlotIndex = request.Data.TryGetProperty("slotIndex", out JsonElement slotElement) ? slotElement.GetInt32() : null,
            ItemId = request.Data.GetProperty("itemId").GetUInt16(),
            Quantity = request.Data.TryGetProperty("quantity", out JsonElement qtyElement) ? qtyElement.GetUInt16() : (ushort)1,
            CrystalNameId = request.Data.TryGetProperty("crystalNameId", out JsonElement nameElement) ? nameElement.GetUInt16() : (ushort)0,
            Rank = request.Data.TryGetProperty("rank", out JsonElement rankElement) ? rankElement.GetByte() : (byte)1,
            Element = request.Data.TryGetProperty("element", out JsonElement elementElement) ? elementElement.GetByte() : (byte)4,
            Buff1Id = request.Data.TryGetProperty("buff1Id", out JsonElement b1i) ? b1i.GetUInt16() : (ushort)0,
            Buff1Value = request.Data.TryGetProperty("buff1Value", out JsonElement b1v) ? b1v.GetUInt16() : (ushort)0,
            Buff2Id = request.Data.TryGetProperty("buff2Id", out JsonElement b2i) ? b2i.GetUInt16() : (ushort)0,
            Buff2Value = request.Data.TryGetProperty("buff2Value", out JsonElement b2v) ? b2v.GetUInt16() : (ushort)0,
            Buff3Id = request.Data.TryGetProperty("buff3Id", out JsonElement b3i) ? b3i.GetUInt16() : (ushort)0,
            Buff3Value = request.Data.TryGetProperty("buff3Value", out JsonElement b3v) ? b3v.GetUInt16() : (ushort)0,
            Buff4Id = request.Data.TryGetProperty("buff4Id", out JsonElement b4i) ? b4i.GetUInt16() : (ushort)0,
            Buff4Value = request.Data.TryGetProperty("buff4Value", out JsonElement b4v) ? b4v.GetUInt16() : (ushort)0,
        };
    }
}

internal sealed class ParseAddOrUpdateItemRequest
{
    public AddOrUpdateItemPayload Run(ClientRequest request)
    {
        return new AddOrUpdateItemPayload
        {
            Category = request.Data.GetProperty("category").GetString() ?? string.Empty,
            SlotIndex = request.Data.TryGetProperty("slotIndex", out JsonElement slotElement) ? slotElement.GetInt32() : null,
            ItemId = request.Data.GetProperty("itemId").GetUInt16(),
            Quantity = request.Data.TryGetProperty("quantity", out JsonElement qtyElement) ? qtyElement.GetUInt16() : (ushort)1,
        };
    }
}
