using System;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units
{
    public sealed class UnitRuntimeState
    {
        public UnitRuntimeState(
            int runtimeId,
            string dataId,
            int value,
            int currentPointIndex,
            int reservedPointIndex,
            bool isMoving,
            Vector3 position)
        {
            RuntimeId = runtimeId;
            DataId = string.IsNullOrWhiteSpace(dataId) ? $"unit_{runtimeId}" : dataId.Trim();
            Value = value;
            CurrentPointIndex = currentPointIndex;
            ReservedPointIndex = reservedPointIndex;
            IsMoving = isMoving;
            Position = position;
        }

        public event Action<UnitRuntimeState> Changed;

        public int RuntimeId { get; }
        public string DataId { get; }
        public int Value { get; private set; }
        public int CurrentPointIndex { get; private set; }
        public int ReservedPointIndex { get; private set; }
        public bool IsMoving { get; private set; }
        public Vector3 Position { get; private set; }

        public void SetValue(int value)
        {
            if (Value == value)
            {
                return;
            }

            Value = value;
            NotifyChanged();
        }

        public void BeginMove(int reservedPointIndex, Vector3 startPosition)
        {
            ReservedPointIndex = reservedPointIndex;
            Position = startPosition;
            IsMoving = true;
            NotifyChanged();
        }

        public void SetPosition(Vector3 position)
        {
            Position = position;
        }

        public void CompleteMove(Vector3 finalPosition)
        {
            CurrentPointIndex = ReservedPointIndex;
            Position = finalPosition;
            IsMoving = false;
            NotifyChanged();
        }

        private void NotifyChanged()
        {
            Changed?.Invoke(this);
        }
    }
}
