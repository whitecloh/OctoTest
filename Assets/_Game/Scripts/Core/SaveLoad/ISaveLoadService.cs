namespace OctoGames.TestTask.Core.SaveLoad
{
    public interface ISaveLoadService
    {
        bool Save<T>(string fileName, T data);
        SaveLoadResult<T> Load<T>(string fileName, T defaultValue = default);
        bool Exists(string fileName);
        bool Delete(string fileName);
    }
}
