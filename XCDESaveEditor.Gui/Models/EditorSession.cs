using XCDESave;

namespace XCDESaveEditor.Gui.Models;

public sealed class EditorSession
{
    public XCDESaveData? SaveData;
    public string FilePath = string.Empty;
    public bool IsLoaded = false;
    public bool BackupCreated = false;
}
