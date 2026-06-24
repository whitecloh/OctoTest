namespace OctoGames.TestTask.Core.SaveLoad
{
    public readonly struct SaveLoadResult<T>
    {
        private SaveLoadResult(bool success, T data, string error)
        {
            Success = success;
            Data = data;
            Error = error;
        }

        public bool Success { get; }
        public T Data { get; }
        public string Error { get; }

        public static SaveLoadResult<T> Succeeded(T data)
        {
            return new SaveLoadResult<T>(true, data, string.Empty);
        }

        public static SaveLoadResult<T> Failed(T data, string error)
        {
            return new SaveLoadResult<T>(false, data, error ?? string.Empty);
        }
    }
}
