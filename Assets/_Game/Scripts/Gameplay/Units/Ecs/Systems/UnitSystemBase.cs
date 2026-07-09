using Leopotam.EcsLite;
using OctoGames.TestTask.App.Bootstrap;
using OctoGames.TestTask.Gameplay.Units.Data;

namespace OctoGames.TestTask.Gameplay.Units.Ecs.Systems
{
    public abstract class UnitSystemBase : IEcsInitSystem
    {
        protected EcsWorld World;
        protected GameServices Services;
        protected EcsFilter UnitsFilter;
        protected EcsPool<UnitComponent> UnitPool;
        protected EcsPool<UnitValueComponent> ValuePool;
        protected EcsPool<UnitGridPositionComponent> GridPositionPool;
        protected EcsPool<UnitWorldPositionComponent> WorldPositionPool;
        protected EcsPool<UnitMovingComponent> MovingPool;

        public virtual void Init(IEcsSystems systems)
        {
            World = systems.GetWorld();
            Services = systems.GetShared<GameServices>();
            UnitsFilter = World
                .Filter<UnitComponent>()
                .Inc<UnitValueComponent>()
                .Inc<UnitGridPositionComponent>()
                .Inc<UnitWorldPositionComponent>()
                .End();
            UnitPool = World.GetPool<UnitComponent>();
            ValuePool = World.GetPool<UnitValueComponent>();
            GridPositionPool = World.GetPool<UnitGridPositionComponent>();
            WorldPositionPool = World.GetPool<UnitWorldPositionComponent>();
            MovingPool = World.GetPool<UnitMovingComponent>();
        }

        protected void ClearUnits()
        {
            foreach (int entity in UnitsFilter)
            {
                World.DelEntity(entity);
            }

            Services.UnitGrid.Clear();
        }

        protected bool TrySpawnRandomUnit()
        {
            if (Services.UnitQuery.Count >= Services.Config.MaxUnits ||
                !Services.UnitGrid.TryReserveFirstFree(out int pointIndex))
            {
                return false;
            }

            UnitDefinition definition = Services.UnitCatalog.GetRandom();
            CreateUnit(
                Services.NextUnitRuntimeId++,
                definition.Id,
                definition.StartValue,
                pointIndex);
            return true;
        }

        protected void CreateUnit(int runtimeId, string dataId, int value, int pointIndex)
        {
            int entity = World.NewEntity();
            ref UnitComponent unit = ref UnitPool.Add(entity);
            unit.RuntimeId = runtimeId;
            unit.DataId = NormalizeDataId(dataId);

            ref UnitValueComponent unitValue = ref ValuePool.Add(entity);
            unitValue.Value = value;

            ref UnitGridPositionComponent gridPosition = ref GridPositionPool.Add(entity);
            gridPosition.CurrentPointIndex = pointIndex;
            gridPosition.ReservedPointIndex = pointIndex;

            ref UnitWorldPositionComponent worldPosition = ref WorldPositionPool.Add(entity);
            worldPosition.Position = Services.UnitGrid.GetWorldPosition(pointIndex);
        }

        protected void StartNewGame()
        {
            ClearUnits();
            Services.NextUnitRuntimeId = 1;

            for (int i = 0; i < Services.Config.InitialUnits; i++)
            {
                TrySpawnRandomUnit();
            }

            Services.MarkUnitsSaveDirty();
        }

        protected string NormalizeDataId(string dataId)
        {
            if (string.IsNullOrWhiteSpace(dataId))
            {
                return Services.UnitCatalog.FirstValid.Id;
            }

            string normalized = dataId.Trim();
            return Services.UnitCatalog.Contains(normalized)
                ? normalized
                : Services.UnitCatalog.FirstValid.Id;
        }
    }
}
