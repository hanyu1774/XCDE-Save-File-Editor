using XCDESave;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class ResolvePartyMember
{
    public PartyMember? Run(XCDESaveData saveData, int characterId)
    {
        if (characterId < 1 || characterId > 15)
        {
            return null;
        }

        return saveData.PartyMembers[characterId];
    }
}
