namespace OctoGames.TestTask.Gameplay.Units.Runtime
{
    public sealed class UnitStatsService
    {
        public UnitStats Calculate(UnitQuery unitQuery, int maxCount)
        {
            if (unitQuery == null)
            {
                return new UnitStats(0, maxCount, 0);
            }

            return unitQuery.CalculateStats(maxCount);
        }
    }
}
