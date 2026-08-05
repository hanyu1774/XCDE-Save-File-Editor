using System;
using XCDESaveEditor.Gui.Models;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class ParseUnknownBlockHex
{
    public HexParseResult Run(string blockName, string hexValue)
    {
        int expectedByteLength = blockName switch
        {
            "unk1" => 4,
            "unk2" => 4,
            "unk3" => 12,
            "unk4" => 37,
            "unk5" => 64,
            _ => -1,
        };

        if (expectedByteLength == -1)
        {
            return new HexParseResult { Success = false, ErrorMessage = "Unknown block name: " + blockName };
        }

        byte[] parsedBytes;

        try
        {
            parsedBytes = Convert.FromHexString(hexValue);
        }
        catch (FormatException)
        {
            return new HexParseResult { Success = false, ErrorMessage = "Value is not valid hexadecimal text." };
        }

        if (parsedBytes.Length != expectedByteLength)
        {
            string message = $"Block {blockName} needs exactly {expectedByteLength} bytes ({expectedByteLength * 2} hex characters), got {parsedBytes.Length}.";
            return new HexParseResult { Success = false, ErrorMessage = message };
        }

        return new HexParseResult { Success = true, Bytes = parsedBytes };
    }
}
