using XCDESave;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class AddCharacterToParty
{
    public bool Run(Party party, Character character)
    {
        for (int i = 0; i < party.PartyMembersCount; i++)
        {
            if (party.Characters[i] == character)
            {
                return false;
            }
        }

        if (party.PartyMembersCount >= party.Characters.Length)
        {
            return false;
        }

        party.Characters[party.PartyMembersCount] = character;
        party.PartyMembersCount = (byte)(party.PartyMembersCount + 1);
        return true;
    }
}

internal sealed class RemoveCharacterFromParty
{
    public bool Run(Party party, Character character)
    {
        int foundIndex = -1;

        for (int i = 0; i < party.PartyMembersCount; i++)
        {
            if (party.Characters[i] == character)
            {
                foundIndex = i;
                break;
            }
        }

        if (foundIndex == -1)
        {
            return false;
        }

        for (int i = foundIndex; i < party.PartyMembersCount - 1; i++)
        {
            party.Characters[i] = party.Characters[i + 1];
        }

        party.Characters[party.PartyMembersCount - 1] = Character.None;
        party.PartyMembersCount = (byte)(party.PartyMembersCount - 1);
        return true;
    }
}

internal sealed class SetCharacterLevel
{
    public void Run(PartyMember member, uint newLevel)
    {
        member.Level = newLevel;
    }
}

internal sealed class SetCharacterAp
{
    public void Run(PartyMember member, uint newAp)
    {
        member.AP = newAp;
    }
}

internal sealed class SetCharacterArtSlot
{
    public void Run(PartyMember member, int slotIndex, ushort artId, bool useMonadoSet)
    {
        ushort[] targetArray = useMonadoSet ? member.MonadoArts : member.Arts;
        targetArray[slotIndex] = artId;
    }
}

internal sealed class SetPartyMemberUnknownBlock
{
    public void Run(PartyMember member, string blockName, byte[] newBytes)
    {
        switch (blockName)
        {
            case "unk1":
                member.Unk_1 = newBytes;
                break;
            case "unk2":
                member.Unk_2 = newBytes;
                break;
            case "unk3":
                member.Unk_3 = newBytes;
                break;
            case "unk4":
                member.Unk_4 = newBytes;
                break;
            case "unk5":
                member.Unk_5 = newBytes;
                break;
        }
    }
}
