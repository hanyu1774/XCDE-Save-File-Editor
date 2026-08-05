using XCDESave;

namespace XCDESaveEditor.Gui.Flows;

internal sealed class LoadSaveFile
{
    public XCDESaveData Run(string filePath)
    {
        byte[] rawBytes = File.ReadAllBytes(filePath);
        XCDESaveData saveData = XCDESaveSerialization.Deserialize(rawBytes);
        return saveData;
    }
}

internal sealed class CreateBackupFile
{
    public string Run(string filePath)
    {
        string backupPath = filePath + ".backup";

        if (!File.Exists(backupPath))
        {
            File.Copy(filePath, backupPath);
        }

        return backupPath;
    }
}

internal sealed class WriteSaveFile
{
    public void Run(XCDESaveData saveData, string filePath)
    {
        byte[] rawBytes = XCDESaveSerialization.Serialize(saveData);
        File.WriteAllBytes(filePath, rawBytes);
    }
}
