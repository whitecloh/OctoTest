using System.Collections.Generic;

namespace OctoGames.TestTask.Gameplay.Units
{
    public sealed class UnitMovementPlanner
    {
        private UnitRuntimeState[] _movableOccupants = new UnitRuntimeState[0];
        private bool[] _canMoveFromPoint = new bool[0];
        private readonly Queue<int> _freePoints = new ();

        public void BuildMovePlan(IReadOnlyList<UnitRuntimeState> units, UnitSpawnGrid grid, List<UnitMovePlan> result)
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
            EnsureCapacity(pointCount);
            ClearBuffers(pointCount);

            for (int i = 0; i < units.Count; i++)
            {
                UnitRuntimeState unit = units[i];
                if (unit == null || unit.IsMoving || !IsUsablePoint(unit.CurrentPointIndex, pointCount))
                {
                    continue;
                }

                _movableOccupants[unit.CurrentPointIndex] ??= unit;
            }

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                if (!grid.IsReserved(pointIndex))
                {
                    _freePoints.Enqueue(pointIndex);
                }
            }

            while (_freePoints.Count > 0)
            {
                int freePointIndex = _freePoints.Dequeue();
                int previousPointIndex = GetPreviousPointIndex(freePointIndex, pointCount);
                UnitRuntimeState unit = _movableOccupants[previousPointIndex];
                if (unit == null || _canMoveFromPoint[previousPointIndex])
                {
                    continue;
                }

                _canMoveFromPoint[previousPointIndex] = true;
                _freePoints.Enqueue(previousPointIndex);
            }

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                if (!_canMoveFromPoint[pointIndex])
                {
                    continue;
                }

                UnitRuntimeState unit = _movableOccupants[pointIndex];
                if (unit != null)
                {
                    result.Add(new UnitMovePlan(unit, GetNextPointIndex(pointIndex, pointCount)));
                }
            }
        }

        private void EnsureCapacity(int pointCount)
        {
            if (_movableOccupants.Length < pointCount)
            {
                _movableOccupants = new UnitRuntimeState[pointCount];
            }

            if (_canMoveFromPoint.Length < pointCount)
            {
                _canMoveFromPoint = new bool[pointCount];
            }
        }

        private void ClearBuffers(int pointCount)
        {
            System.Array.Clear(_movableOccupants, 0, pointCount);
            System.Array.Clear(_canMoveFromPoint, 0, pointCount);
            _freePoints.Clear();
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
