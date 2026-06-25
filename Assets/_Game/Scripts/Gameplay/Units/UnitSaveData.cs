using System;

namespace OctoGames.TestTask.Gameplay.Units
{
    using UnityEngine;

    [Serializable]
    public sealed class UnitSaveData
    {
        [SerializeField] private int runtimeId;
        [SerializeField] private string dataId;
        [SerializeField] private int value;
        [SerializeField] private int pointIndex;

        public int RuntimeId
        {
            get => runtimeId;
            set => runtimeId = value;
        }

        public string DataId
        {
            get => dataId;
            set => dataId = value;
        }

        public int Value
        {
            get => value;
            set => this.value = value;
        }

        public int PointIndex
        {
            get => pointIndex;
            set => pointIndex = value;
        }

        public void Set(int runtimeId, string dataId, int value, int pointIndex)
        {
            RuntimeId = runtimeId;
            DataId = dataId;
            Value = value;
            PointIndex = pointIndex;
        }

        public void Clear()
        {
            runtimeId = 0;
            dataId = string.Empty;
            value = 0;
            pointIndex = -1;
        }
    }
}
