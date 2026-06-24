using System.Collections.Generic;

namespace OctoGames.TestTask.Gameplay.Units
{
    public sealed class UnitStatsService
    {
        public UnitStats Calculate(IReadOnlyList<UnitRuntimeState> units, int maxCount)
        {
            if (units == null || units.Count == 0)
            {
                return new UnitStats(0, maxCount, 0);
            }

            int unitCount = 0;
            int totalValue = 0;

            for (int i = 0; i < units.Count; i++)
            {
                UnitRuntimeState unit = units[i];
                if (unit == null)
                {
                    continue;
                }

                unitCount++;
                totalValue += unit.Value;
            }

            return new UnitStats(unitCount, maxCount, totalValue);
        }
    }
}
