using XCDESave;
using XCDESaveEditor.Gui.Flows;
using XCDESaveEditor.Gui.Models;

namespace XCDESaveEditor.Gui.Workflows;

public sealed class LoadFileWorkflow
{
    public ClientResponse Run(EditorSession session, ClientRequest request)
    {
        // Flows
        ParseFilePathRequest parseFilePathRequest = new();
        LoadSaveFile loadSaveFile = new();
        CreateBackupFile createBackupFile = new();
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        string filePath = parseFilePathRequest.Run(request);

        if (string.IsNullOrWhiteSpace(filePath) || !System.IO.File.Exists(filePath))
        {
            return buildErrorResponse.Run("File not found.");
        }

        XCDESaveData saveData;
        string backupPath;

        try
        {
            saveData = loadSaveFile.Run(filePath);
            backupPath = createBackupFile.Run(filePath);
        }
        catch (System.Exception exception)
        {
            return buildErrorResponse.Run("Could not read save file: " + exception.Message);
        }

        session.SaveData = saveData;
        session.FilePath = filePath;
        session.IsLoaded = true;
        session.BackupCreated = true;

        ClientResponse response = buildSuccessResponse.Run(saveData, $"Loaded save file. Backup created at {backupPath}.");
        response.FilePath = filePath;
        return response;
    }
}

public sealed class SaveFileWorkflow
{
    public ClientResponse Run(EditorSession session)
    {
        // Flows
        WriteSaveFile writeSaveFile = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        try
        {
            writeSaveFile.Run(session.SaveData, session.FilePath);
        }
        catch (System.Exception exception)
        {
            return buildErrorResponse.Run("Could not write save file: " + exception.Message);
        }

        return new ClientResponse { Success = true, Message = "Save file written to " + session.FilePath, FilePath = session.FilePath };
    }
}

public sealed class GetSnapshotWorkflow
{
    public ClientResponse Run(EditorSession session)
    {
        // Flows
        BuildSuccessResponse buildSuccessResponse = new();
        BuildErrorResponse buildErrorResponse = new();

        // Orchestration
        if (!session.IsLoaded || session.SaveData is null)
        {
            return buildErrorResponse.Run("No save file is currently loaded.");
        }

        return buildSuccessResponse.Run(session.SaveData, string.Empty);
    }
}
