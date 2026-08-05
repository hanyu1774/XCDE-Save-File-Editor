using XCDESave;
using XCDESaveEditor.Gui.Models;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class ResolveEquipBox
{
    public EquipBoxResolution Run(XCDESaveData saveData, string category)
    {
        return category switch
        {
            "weapon" => new EquipBoxResolution { Success = true, Box = saveData.Weapons, ItemType = ItemType.Weapon },
            "headArmor" => new EquipBoxResolution { Success = true, Box = saveData.HeadArmour, ItemType = ItemType.HeadArmour },
            "torsoArmor" => new EquipBoxResolution { Success = true, Box = saveData.TorsoArmour, ItemType = ItemType.TorsoArmour },
            "armArmor" => new EquipBoxResolution { Success = true, Box = saveData.ArmArmour, ItemType = ItemType.ArmArmour },
            "legArmor" => new EquipBoxResolution { Success = true, Box = saveData.LegArmour, ItemType = ItemType.LegArmour },
            "footArmor" => new EquipBoxResolution { Success = true, Box = saveData.FootArmour, ItemType = ItemType.FootArmour },
            _ => new EquipBoxResolution { Success = false },
        };
    }
}

internal sealed class ResolveCrystalBox
{
    public CrystalBoxResolution Run(XCDESaveData saveData, string category)
    {
        return category switch
        {
            "gem" => new CrystalBoxResolution { Success = true, Box = saveData.Gems, ItemType = ItemType.Gem },
            "crystal" => new CrystalBoxResolution { Success = true, Box = saveData.Crystals, ItemType = ItemType.Crystal },
            _ => new CrystalBoxResolution { Success = false },
        };
    }
}

internal sealed class ResolveItemBox
{
    public ItemBoxResolution Run(XCDESaveData saveData, string category)
    {
        return category switch
        {
            "collectable" => new ItemBoxResolution { Success = true, Box = saveData.Collectables, ItemType = ItemType.Collectable },
            "material" => new ItemBoxResolution { Success = true, Box = saveData.Materials, ItemType = ItemType.Material },
            "keyItem" => new ItemBoxResolution { Success = true, Box = saveData.KeyItems, ItemType = ItemType.KeyItem },
            "artsManual" => new ItemBoxResolution { Success = true, Box = saveData.ArtsManuals, ItemType = ItemType.ArtsManual },
            _ => new ItemBoxResolution { Success = false },
        };
    }
}
