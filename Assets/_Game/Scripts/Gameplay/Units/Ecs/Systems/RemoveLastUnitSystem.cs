using Leopotam.EcsLite;

namespace OctoGames.TestTask.Gameplay.Units.Ecs.Systems
{
    public sealed class RemoveLastUnitSystem : UnitSystemBase, IEcsRunSystem
    {
        private EcsFilter _requestFilter;

        public override void Init(IEcsSystems systems)
        {
            base.Init(systems);
            _requestFilter = World.Filter<RemoveLastUnitRequest>().End();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int _ in _requestFilter)
            {
                if (TryRemoveLastUnit())
                {
                    Services.MarkUnitsSaveDirty();
                }
            }
        }

        private bool TryRemoveLastUnit()
        {
            int targetEntity = -1;
            int targetRuntimeId = -1;
            foreach (int entity in UnitsFilter)
            {
                int runtimeId = UnitPool.Get(entity).RuntimeId;
                if (runtimeId > targetRuntimeId)
                {
                    targetRuntimeId = runtimeId;
                    targetEntity = entity;
                }
            }

            if (targetEntity < 0)
            {
                return false;
            }

            UnitGridPositionComponent gridPosition = GridPositionPool.Get(targetEntity);
            Services.UnitGrid.Release(gridPosition.CurrentPointIndex);
            if (gridPosition.ReservedPointIndex != gridPosition.CurrentPointIndex)
            {
                Services.UnitGrid.Release(gridPosition.ReservedPointIndex);
            }

            World.DelEntity(targetEntity);
            return true;
        }
    }
}
