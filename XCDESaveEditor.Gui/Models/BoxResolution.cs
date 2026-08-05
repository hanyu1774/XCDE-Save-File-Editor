using XCDESave;

namespace XCDESaveEditor.Gui.Models;

public sealed class EquipBoxResolution
{
    public bool Success;
    public EquipItem[] Box = System.Array.Empty<EquipItem>();
    public ItemType ItemType;
}

public sealed class CrystalBoxResolution
{
    public bool Success;
    public CrystalItem[] Box = System.Array.Empty<CrystalItem>();
    public ItemType ItemType;
}

public sealed class ItemBoxResolution
{
    public bool Success;
    public Item[] Box = System.Array.Empty<Item>();
    public ItemType ItemType;
}
