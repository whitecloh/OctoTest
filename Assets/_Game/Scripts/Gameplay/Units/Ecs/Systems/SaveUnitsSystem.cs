using Leopotam.EcsLite;
using OctoGames.TestTask.Gameplay.Units.Persistence;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units.Ecs.Systems
{
    public sealed class SaveUnitsSystem : UnitSystemBase, IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            if (!Services.ConsumeUnitsSaveDirty())
            {
                return;
            }

            UnitsSaveData saveData = BuildSaveData();
            if (!Services.SaveLoadService.Save(Services.Config.UnitsSaveFileName, saveData))
            {
                Debug.LogWarning($"{nameof(SaveUnitsSystem)} failed to save units state.");
            }
        }

        private UnitsSaveData BuildSaveData()
        {
            UnitsSaveData saveData = new()
            {
                SaveVersion = UnitsSaveData_v_1_0.CurrentVersion,
                NextRuntimeId = Services.NextUnitRuntimeId
            };

            foreach (int entity in UnitsFilter)
            {
                UnitComponent unit = UnitPool.Get(entity);
                UnitValueComponent value = ValuePool.Get(entity);
                UnitGridPositionComponent grid = GridPositionPool.Get(entity);
                int pointIndex = MovingPool.Has(entity)
                    ? grid.ReservedPointIndex
                    : grid.CurrentPointIndex;
                saveData.AddUnit().Set(unit.RuntimeId, unit.DataId, value.Value, pointIndex);
            }

            return saveData;
        }
    }
}
