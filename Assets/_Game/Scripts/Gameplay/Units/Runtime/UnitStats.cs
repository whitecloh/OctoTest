namespace OctoGames.TestTask.Gameplay.Units.Runtime
{
    public readonly struct UnitStats
    {
        public UnitStats(int totalCount, int maxCount, int totalValue)
        {
            TotalCount = totalCount;
            MaxCount = maxCount;
            TotalValue = totalValue;
            AverageValue = totalCount > 0 ? (float)totalValue / totalCount : 0f;
        }

        public int TotalCount { get; }
        public int MaxCount { get; }
        public int TotalValue { get; }
        public float AverageValue { get; }
    }
}
