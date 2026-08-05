using XCDESave;
using XCDESaveEditor.Gui.Models;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class BuildSaveSnapshot
{
    public SaveSnapshot Run(XCDESaveData saveData)
    {
        SaveSnapshot snapshot = new()
        {
            Money = saveData.Money,
            Noponstones = saveData.Noponstones,
        };

        for (int i = 0; i < saveData.Party.PartyMembersCount; i++)
        {
            snapshot.PartyCharacterIds.Add((int)saveData.Party.Characters[i]);
        }

        int namedCharacterCount = 16;
        for (int characterId = 1; characterId < namedCharacterCount; characterId++)
        {
            PartyMember member = saveData.PartyMembers[characterId];
            Character character = (Character)characterId;

            snapshot.PartyMembers.Add(new PartyMemberSnapshot
            {
                CharacterId = characterId,
                CharacterName = character.ToString(),
                Level = member.Level,
                Exp = member.EXP,
                Ap = member.AP,
                AffinityCoins = member.AffinityCoins,
                Arts = (ushort[])member.Arts.Clone(),
                MonadoArts = (ushort[])member.MonadoArts.Clone(),
                Unk1Hex = BytesToHex(member.Unk_1),
                Unk2Hex = BytesToHex(member.Unk_2),
                Unk3Hex = BytesToHex(member.Unk_3),
                Unk4Hex = BytesToHex(member.Unk_4),
                Unk5Hex = BytesToHex(member.Unk_5),
            });
        }

        CollectEquipBox(saveData.Weapons, snapshot.Weapons);
        CollectEquipBox(saveData.HeadArmour, snapshot.HeadArmor);
        CollectEquipBox(saveData.TorsoArmour, snapshot.TorsoArmor);
        CollectEquipBox(saveData.ArmArmour, snapshot.ArmArmor);
        CollectEquipBox(saveData.LegArmour, snapshot.LegArmor);
        CollectEquipBox(saveData.FootArmour, snapshot.FootArmor);

        CollectCrystalBox(saveData.Gems, snapshot.Gems);
        CollectCrystalBox(saveData.Crystals, snapshot.Crystals);

        CollectItemBox(saveData.Collectables, snapshot.Collectables);
        CollectItemBox(saveData.Materials, snapshot.Materials);
        CollectItemBox(saveData.KeyItems, snapshot.KeyItems);
        CollectItemBox(saveData.ArtsManuals, snapshot.ArtsManuals);

        return snapshot;
    }

    private static void CollectEquipBox(EquipItem[] box, List<EquipItemSnapshot> target)
    {
        for (int i = 0; i < box.Length; i++)
        {
            EquipItem item = box[i];

            if (!item.Exists)
            {
                continue;
            }

            target.Add(new EquipItemSnapshot
            {
                SlotIndex = i,
                ItemId = item.FullID.ID,
                Quantity = item.Quantity,
                GemSlots = item.GemSlots,
                Gem1Id = item.Gem1.ID,
                Gem2Id = item.Gem2.ID,
                Gem3Id = item.Gem3.ID,
            });
        }
    }

    private static void CollectCrystalBox(CrystalItem[] box, List<CrystalItemSnapshot> target)
    {
        for (int i = 0; i < box.Length; i++)
        {
            CrystalItem item = box[i];

            if (!item.Exists)
            {
                continue;
            }

            target.Add(new CrystalItemSnapshot
            {
                SlotIndex = i,
                ItemId = item.FullID.ID,
                Quantity = item.Quantity,
                CrystalNameId = item.CrystalNameID,
                Rank = item.Rank,
                Element = item.Element,
                Buff1Id = item.Buff1ID,
                Buff1Value = item.Buff1Value,
                Buff2Id = item.Buff2ID,
                Buff2Value = item.Buff2Value,
                Buff3Id = item.Buff3ID,
                Buff3Value = item.Buff3Value,
                Buff4Id = item.Buff4ID,
                Buff4Value = item.Buff4Value,
            });
        }
    }

    private static void CollectItemBox(Item[] box, List<ItemSnapshot> target)
    {
        for (int i = 0; i < box.Length; i++)
        {
            Item item = box[i];

            if (!item.Exists)
            {
                continue;
            }

            target.Add(new ItemSnapshot
            {
                SlotIndex = i,
                ItemId = item.FullID.ID,
                Quantity = item.Quantity,
            });
        }
    }

    private static string BytesToHex(byte[] bytes)
    {
        return Convert.ToHexString(bytes);
    }
}
