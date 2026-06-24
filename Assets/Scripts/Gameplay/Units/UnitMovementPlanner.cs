using System.Collections.Generic;

namespace OctoGames.TestTask.Gameplay.Units
{
    public static class UnitMovementPlanner
    {
        public static void BuildMovePlan(IReadOnlyList<UnitRuntimeState> units, UnitSpawnGrid grid, List<UnitMovePlan> result)
        {
            if (result == null)
            {
                return;
            }

            result.Clear();

            if (units == null || grid == null || grid.Count == 0)
            {
                return;
            }

            int pointCount = grid.Count;
            UnitRuntimeState[] movableOccupants = new UnitRuntimeState[pointCount];
            bool[] canMoveFromPoint = new bool[pointCount];
            Queue<int> freePoints = new Queue<int>();

            for (int i = 0; i < units.Count; i++)
            {
                UnitRuntimeState unit = units[i];
                if (unit == null || unit.IsMoving || !IsUsablePoint(unit.CurrentPointIndex, pointCount))
                {
                    continue;
                }

                movableOccupants[unit.CurrentPointIndex] ??= unit;
            }

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                if (!grid.IsReserved(pointIndex))
                {
                    freePoints.Enqueue(pointIndex);
                }
            }

            while (freePoints.Count > 0)
            {
                int freePointIndex = freePoints.Dequeue();
                int previousPointIndex = GetPreviousPointIndex(freePointIndex, pointCount);
                UnitRuntimeState unit = movableOccupants[previousPointIndex];
                if (unit == null || canMoveFromPoint[previousPointIndex])
                {
                    continue;
                }

                canMoveFromPoint[previousPointIndex] = true;
                freePoints.Enqueue(previousPointIndex);
            }

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                if (!canMoveFromPoint[pointIndex])
                {
                    continue;
                }

                UnitRuntimeState unit = movableOccupants[pointIndex];
                if (unit != null)
                {
                    result.Add(new UnitMovePlan(unit, GetNextPointIndex(pointIndex, pointCount)));
                }
            }
        }

        private static int GetNextPointIndex(int pointIndex, int pointCount)
        {
            return (pointIndex + 1) % pointCount;
        }

        private static int GetPreviousPointIndex(int pointIndex, int pointCount)
        {
            return pointIndex <= 0 ? pointCount - 1 : pointIndex - 1;
        }

        private static bool IsUsablePoint(int pointIndex, int pointCount)
        {
            return pointIndex >= 0 && pointIndex < pointCount;
        }
    }
}
