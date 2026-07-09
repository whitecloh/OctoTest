using Leopotam.EcsLite;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units.Ecs.Systems
{
    public sealed class UnitMovementSystem : UnitSystemBase, IEcsRunSystem
    {
        private EcsFilter _movingUnitsFilter;

        public override void Init(IEcsSystems systems)
        {
            base.Init(systems);
            _movingUnitsFilter = World
                .Filter<UnitMovingComponent>()
                .Inc<UnitWorldPositionComponent>()
                .Inc<UnitGridPositionComponent>()
                .End();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int entity in _movingUnitsFilter)
            {
                ref UnitMovingComponent moving = ref MovingPool.Get(entity);
                ref UnitWorldPositionComponent position = ref WorldPositionPool.Get(entity);
                ref UnitGridPositionComponent gridPosition = ref GridPositionPool.Get(entity);

                Vector3 nextPosition = Vector3.MoveTowards(
                    position.Position,
                    moving.TargetPosition,
                    Services.Config.UnitMoveSpeed * Services.DeltaTime);
                position.Position = nextPosition;

                if ((nextPosition - moving.TargetPosition).sqrMagnitude > 0.0001f)
                {
                    continue;
                }

                int previousPointIndex = gridPosition.CurrentPointIndex;
                gridPosition.CurrentPointIndex = gridPosition.ReservedPointIndex;
                position.Position = moving.TargetPosition;
                MovingPool.Del(entity);

                if (previousPointIndex != gridPosition.CurrentPointIndex)
                {
                    Services.UnitGrid.Release(previousPointIndex);
                }
            }
        }
    }
}
