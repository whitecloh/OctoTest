using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units
{
    public sealed class UnitSpawnGrid
    {
        private readonly int[] _reservationCounts;

        public UnitSpawnGrid(int columns, int rows, float spacing, Vector3 origin)
        {
            Columns = Mathf.Max(1, columns);
            Rows = Mathf.Max(1, rows);
            Spacing = Mathf.Max(0.1f, spacing);
            Origin = origin;
            _reservationCounts = new int[Columns * Rows];
        }

        private int Columns { get; }
        private int Rows { get; }
        private float Spacing { get; }
        private Vector3 Origin { get; set; }
        public int Count => _reservationCounts.Length;

        public void SetOrigin(Vector3 origin)
        {
            Origin = origin;
        }

        public void Clear()
        {
            for (int i = 0; i < _reservationCounts.Length; i++)
            {
                _reservationCounts[i] = 0;
            }
        }

        public bool TryReserveFirstFree(out int pointIndex)
        {
            for (int i = 0; i < _reservationCounts.Length; i++)
            {
                if (_reservationCounts[i] > 0)
                {
                    continue;
                }

                _reservationCounts[i] = 1;
                pointIndex = i;
                return true;
            }

            pointIndex = -1;
            return false;
        }

        public bool AddReservation(int pointIndex)
        {
            if (!IsValidIndex(pointIndex))
            {
                return false;
            }

            _reservationCounts[pointIndex]++;
            return true;
        }

        public bool IsReserved(int pointIndex)
        {
            return IsValidIndex(pointIndex) && _reservationCounts[pointIndex] > 0;
        }

        public void Release(int pointIndex)
        {
            if (IsValidIndex(pointIndex))
            {
                _reservationCounts[pointIndex] = Mathf.Max(0, _reservationCounts[pointIndex] - 1);
            }
        }

        public Vector3 GetWorldPosition(int pointIndex)
        {
            int safeIndex = Mathf.Clamp(pointIndex, 0, _reservationCounts.Length - 1);
            int x = safeIndex % Columns;
            int y = safeIndex / Columns;
            return Origin + new Vector3(x * Spacing, y * Spacing, 0f);
        }

        private bool IsValidIndex(int pointIndex)
        {
            return pointIndex >= 0 && pointIndex < _reservationCounts.Length;
        }
    }
}
