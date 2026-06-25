namespace OctoGames.TestTask.Core.SaveLoad
{
    public interface ISaveSerializer
    {
        string Serialize<T>(T data);
        T Deserialize<T>(string json);
    }
}
