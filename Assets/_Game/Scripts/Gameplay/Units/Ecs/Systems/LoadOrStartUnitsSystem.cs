using System;
using System.Collections.Generic;
using Leopotam.EcsLite;
using OctoGames.TestTask.Core.SaveLoad;
using OctoGames.TestTask.Gameplay.Units.Persistence;

namespace OctoGames.TestTask.Gameplay.Units.Ecs.Systems
{
    public sealed class LoadOrStartUnitsSystem : UnitSystemBase, IEcsRunSystem
    {
        private readonly UnitsSaveMigrationPipeline _migrationPipeline = new();
        private EcsFilter _requestFilter;

        public override void Init(IEcsSystems systems)
        {
            base.Init(systems);
            _requestFilter = World.Filter<LoadOrCreateUnitsRequest>().End();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int _ in _requestFilter)
            {
                LoadOrStart();
                return;
            }
        }

        private void LoadOrStart()
        {
            SaveLoadResult<UnitsSaveData> result =
                Services.SaveLoadService.Load<UnitsSaveData>(Services.Config.UnitsSaveFileName);

            if (result is { Success: true, Data: not null })
            {
                Restore(result.Data);
                Services.MarkUnitsSaveDirty();
                return;
            }

            StartNewGame();
        }

        private void Restore(UnitsSaveData saveData)
        {
            ClearUnits();
            _migrationPipeline.MigrateToCurrent(saveData, Services.UnitCatalog);
            Services.NextUnitRuntimeId = saveData.NextRuntimeId;

            bool[] usedPoints = new bool[Services.UnitGrid.Count];
            HashSet<int> usedRuntimeIds = new();
            List<UnitSaveData> savedUnits = saveData.Units;

            for (int i = 0; i < savedUnits.Count && Services.UnitQuery.Count < Services.Config.MaxUnits; i++)
            {
                UnitSaveData savedUnit = savedUnits[i];
                if (savedUnit == null ||
                    !TryReserveSavedPoint(savedUnit.PointIndex, usedPoints))
                {
                    continue;
                }

                int runtimeId = GetUniqueRuntimeId(savedUnit.RuntimeId, usedRuntimeIds);
                CreateUnit(
                    runtimeId,
                    savedUnit.DataId,
                    savedUnit.Value,
                    savedUnit.PointIndex);
                Services.NextUnitRuntimeId = Math.Max(Services.NextUnitRuntimeId, runtimeId + 1);
            }
        }

        private bool TryReserveSavedPoint(int pointIndex, bool[] usedPoints)
        {
            if (pointIndex < 0 ||
                pointIndex >= Services.UnitGrid.Count ||
                usedPoints[pointIndex] ||
                Services.UnitGrid.IsReserved(pointIndex))
            {
                return false;
            }

            Services.UnitGrid.AddReservation(pointIndex);
            usedPoints[pointIndex] = true;
            return true;
        }

        private int GetUniqueRuntimeId(int savedRuntimeId, HashSet<int> usedRuntimeIds)
        {
            int runtimeId = Math.Max(1, savedRuntimeId);
            if (usedRuntimeIds.Add(runtimeId))
            {
                return runtimeId;
            }

            runtimeId = Math.Max(Services.NextUnitRuntimeId, runtimeId + 1);
            while (!usedRuntimeIds.Add(runtimeId))
            {
                runtimeId++;
            }

            return runtimeId;
        }
    }
}
