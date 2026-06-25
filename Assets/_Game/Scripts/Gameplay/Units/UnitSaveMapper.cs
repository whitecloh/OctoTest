using System.Collections.Generic;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units
{
    public static class UnitSaveMapper
    {
        public static UnitsSaveData ToSaveData(IReadOnlyList<UnitRuntimeState> units, int nextRuntimeId)
        {
            UnitsSaveData saveData = new UnitsSaveData
            {
                SaveVersion = UnitsSaveData_v_1_0.CurrentVersion,
                NextRuntimeId = nextRuntimeId
            };

            if (units == null)
            {
                return saveData;
            }

            for (int i = 0; i < units.Count; i++)
            {
                UnitRuntimeState unit = units[i];
                if (unit == null)
                {
                    continue;
                }

                saveData.AddUnit().Set(
                    unit.RuntimeId,
                    unit.DataId,
                    unit.Value,
                    unit.IsMoving ? unit.ReservedPointIndex : unit.CurrentPointIndex);
            }

            return saveData;
        }

        public static void RestoreUnits(
            UnitsSaveData saveData,
            UnitSpawnGrid grid,
            int maxUnits,
            UnitCatalog unitCatalog,
            List<UnitRuntimeState> units,
            ref int nextRuntimeId)
        {
            if (saveData == null || grid == null || units == null)
            {
                return;
            }

            nextRuntimeId = saveData.NextRuntimeId;
            bool[] usedPoints = new bool[grid.Count];
            HashSet<int> usedRuntimeIds = new HashSet<int>();
            List<UnitSaveData> savedUnits = saveData.Units;

            for (int i = 0; i < savedUnits.Count && units.Count < maxUnits; i++)
            {
                UnitSaveData savedUnit = savedUnits[i];
                if (savedUnit == null || !TryReserveSavedUnitPoint(savedUnit.PointIndex, grid, usedPoints))
                {
                    continue;
                }

                int pointIndex = savedUnit.PointIndex;
                int runtimeId = GetUniqueRuntimeId(savedUnit.RuntimeId, usedRuntimeIds, ref nextRuntimeId);

                units.Add(new UnitRuntimeState(
                    runtimeId,
                    NormalizeDataId(savedUnit.DataId, unitCatalog),
                    savedUnit.Value,
                    pointIndex,
                    pointIndex,
                    false,
                    grid.GetWorldPosition(pointIndex)));

                nextRuntimeId = Mathf.Max(nextRuntimeId, runtimeId + 1);
            }
        }

        private static bool TryReserveSavedUnitPoint(int pointIndex, UnitSpawnGrid grid, bool[] usedPoints)
        {
            if (!IsUsablePoint(pointIndex, grid.Count) || usedPoints[pointIndex] || grid.IsReserved(pointIndex))
            {
                return false;
            }

            grid.AddReservation(pointIndex);
            usedPoints[pointIndex] = true;
            return true;
        }

        private static int GetUniqueRuntimeId(int savedRuntimeId, HashSet<int> usedRuntimeIds, ref int nextRuntimeId)
        {
            int runtimeId = Mathf.Max(1, savedRuntimeId);

            if (usedRuntimeIds.Add(runtimeId))
            {
                return runtimeId;
            }

            runtimeId = Mathf.Max(nextRuntimeId, runtimeId + 1);
            while (!usedRuntimeIds.Add(runtimeId))
            {
                runtimeId++;
            }

            nextRuntimeId = Mathf.Max(nextRuntimeId, runtimeId + 1);
            return runtimeId;
        }

        private static string NormalizeDataId(string dataId, UnitCatalog unitCatalog)
        {
            if (string.IsNullOrWhiteSpace(dataId))
            {
                return unitCatalog != null ? unitCatalog.Default.Id : string.Empty;
            }

            string normalizedDataId = dataId.Trim();
            return unitCatalog == null || unitCatalog.Contains(normalizedDataId)
                ? normalizedDataId
                : unitCatalog.Default.Id;
        }

        private static bool IsUsablePoint(int pointIndex, int pointCount)
        {
            return pointIndex >= 0 && pointIndex < pointCount;
        }
    }
}
