using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units.Runtime
{
    public readonly struct UnitSnapshot
    {
        public UnitSnapshot(
            int runtimeId,
            string dataId,
            int value,
            int currentPointIndex,
            int reservedPointIndex,
            bool isMoving,
            Vector3 position)
        {
            RuntimeId = runtimeId;
            DataId = dataId;
            Value = value;
            CurrentPointIndex = currentPointIndex;
            ReservedPointIndex = reservedPointIndex;
            IsMoving = isMoving;
            Position = position;
        }

        public int RuntimeId { get; }
        public string DataId { get; }
        public int Value { get; }
        public int CurrentPointIndex { get; }
        public int ReservedPointIndex { get; }
        public bool IsMoving { get; }
        public Vector3 Position { get; }
    }
}
