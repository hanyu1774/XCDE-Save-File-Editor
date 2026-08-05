namespace XCDESaveEditor.Gui.Models;

public sealed class SaveSnapshot
{
    public uint Money;
    public uint Noponstones;
    public List<int> PartyCharacterIds = new();
    public List<PartyMemberSnapshot> PartyMembers = new();
    public List<EquipItemSnapshot> Weapons = new();
    public List<EquipItemSnapshot> HeadArmor = new();
    public List<EquipItemSnapshot> TorsoArmor = new();
    public List<EquipItemSnapshot> ArmArmor = new();
    public List<EquipItemSnapshot> LegArmor = new();
    public List<EquipItemSnapshot> FootArmor = new();
    public List<CrystalItemSnapshot> Gems = new();
    public List<CrystalItemSnapshot> Crystals = new();
    public List<ItemSnapshot> Collectables = new();
    public List<ItemSnapshot> Materials = new();
    public List<ItemSnapshot> KeyItems = new();
    public List<ItemSnapshot> ArtsManuals = new();
}

public sealed class PartyMemberSnapshot
{
    public int CharacterId;
    public string CharacterName = string.Empty;
    public uint Level;
    public uint Exp;
    public uint Ap;
    public uint AffinityCoins;
    public ushort[] Arts = Array.Empty<ushort>();
    public ushort[] MonadoArts = Array.Empty<ushort>();
    public string Unk1Hex = string.Empty;
    public string Unk2Hex = string.Empty;
    public string Unk3Hex = string.Empty;
    public string Unk4Hex = string.Empty;
    public string Unk5Hex = string.Empty;
}

public sealed class EquipItemSnapshot
{
    public int SlotIndex;
    public ushort ItemId;
    public ushort Quantity;
    public byte GemSlots;
    public ushort Gem1Id;
    public ushort Gem2Id;
    public ushort Gem3Id;
}

public sealed class CrystalItemSnapshot
{
    public int SlotIndex;
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

public sealed class ItemSnapshot
{
    public int SlotIndex;
    public ushort ItemId;
    public ushort Quantity;
}
