using System;
using System.Collections.Generic;

namespace OctoGames.TestTask.Gameplay.Units
{
    using UnityEngine;

    [Serializable]
    public sealed class UnitsSaveData : UnitsSaveData_v_1_0 { }

    [Serializable]
    public class UnitsSaveData_v_1_0
    {
        [SerializeField] private int nextRuntimeId = 1;
        [SerializeField] private List<UnitSaveData> units = new ();

        public int NextRuntimeId
        {
            get => Mathf.Max(1, nextRuntimeId);
            set => nextRuntimeId = Mathf.Max(1, value);
        }

        public List<UnitSaveData> Units
        {
            get
            {
                units ??= new List<UnitSaveData>();
                return units;
            }
        }

        public UnitSaveData AddUnit()
        {
            UnitSaveData unit = new UnitSaveData();
            Units.Add(unit);
            return unit;
        }

        public void Clear()
        {
            nextRuntimeId = 1;
            Units.Clear();
        }
    }
}
