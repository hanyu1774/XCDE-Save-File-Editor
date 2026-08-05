using XCDESave;
using XCDESaveEditor.Gui.Flows;
using XCDESaveEditor.Gui.Models;

namespace XCDESaveEditor.Gui.Workflows;

public sealed class AddCharacterWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseCharacterIdRequest parseCharacterIdRequest = new();
        AddCharacterToParty addCharacterToParty = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        Character character = (Character)parseCharacterIdRequest.Run(request);

        bool wasAdded = addCharacterToParty.Run(saveData.Party, character);

        if (!wasAdded)
        {
            return buildErrorResponse.Run("Character is already in the party, or the party is full (max 12).");
        }

        return buildSuccessResponse.Run(saveData, $"{character} added to the party.");
    }
}

public sealed class RemoveCharacterWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseCharacterIdRequest parseCharacterIdRequest = new();
        RemoveCharacterFromParty removeCharacterFromParty = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        Character character = (Character)parseCharacterIdRequest.Run(request);

        bool wasRemoved = removeCharacterFromParty.Run(saveData.Party, character);

        if (!wasRemoved)
        {
            return buildErrorResponse.Run("Character is not currently in the party.");
        }

        return buildSuccessResponse.Run(saveData, $"{character} removed from the party.");
    }
}

public sealed class SetCharacterLevelWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseSetLevelRequest parseSetLevelRequest = new();
        ResolvePartyMember resolvePartyMember = new();
        SetCharacterLevel setCharacterLevel = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        SetCharacterStatPayload payload = parseSetLevelRequest.Run(request);
        PartyMember? member = resolvePartyMember.Run(saveData, payload.CharacterId);

        if (member is null)
        {
            return buildErrorResponse.Run("Character ID must be between 1 and 15.");
        }

        setCharacterLevel.Run(member, payload.Value);

        return buildSuccessResponse.Run(saveData, "Level updated.");
    }
}

public sealed class SetCharacterApWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseSetApRequest parseSetApRequest = new();
        ResolvePartyMember resolvePartyMember = new();
        SetCharacterAp setCharacterAp = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        SetCharacterStatPayload payload = parseSetApRequest.Run(request);
        PartyMember? member = resolvePartyMember.Run(saveData, payload.CharacterId);

        if (member is null)
        {
            return buildErrorResponse.Run("Character ID must be between 1 and 15.");
        }

        setCharacterAp.Run(member, payload.Value);

        return buildSuccessResponse.Run(saveData, "AP updated.");
    }
}

public sealed class SetCharacterArtSlotWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseSetArtSlotRequest parseSetArtSlotRequest = new();
        ResolvePartyMember resolvePartyMember = new();
        SetCharacterArtSlot setCharacterArtSlot = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        SetArtSlotPayload payload = parseSetArtSlotRequest.Run(request);
        PartyMember? member = resolvePartyMember.Run(saveData, payload.CharacterId);

        if (member is null)
        {
            return buildErrorResponse.Run("Character ID must be between 1 and 15.");
        }

        if (payload.SlotIndex < 0 || payload.SlotIndex > 8)
        {
            return buildErrorResponse.Run("Art slot index must be between 0 and 8.");
        }

        setCharacterArtSlot.Run(member, payload.SlotIndex, payload.ArtId, payload.UseMonadoSet);

        return buildSuccessResponse.Run(saveData, "Art slot updated.");
    }
}

public sealed class SetCharacterUnknownBlockWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseSetUnknownBlockRequest parseSetUnknownBlockRequest = new();
        ResolvePartyMember resolvePartyMember = new();
        ParseUnknownBlockHex parseUnknownBlockHex = new();
        SetPartyMemberUnknownBlock setPartyMemberUnknownBlock = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        XCDESaveData saveData = session.SaveData;
        SetUnknownBlockPayload payload = parseSetUnknownBlockRequest.Run(request);
        PartyMember? member = resolvePartyMember.Run(saveData, payload.CharacterId);

        if (member is null)
        {
            return buildErrorResponse.Run("Character ID must be between 1 and 15.");
        }

        HexParseResult hexResult = parseUnknownBlockHex.Run(payload.BlockName, payload.HexValue);

        if (!hexResult.Success)
        {
            return buildErrorResponse.Run(hexResult.ErrorMessage);
        }

        setPartyMemberUnknownBlock.Run(member, payload.BlockName, hexResult.Bytes);

        return buildSuccessResponse.Run(saveData, "Unknown block updated.");
    }
}
