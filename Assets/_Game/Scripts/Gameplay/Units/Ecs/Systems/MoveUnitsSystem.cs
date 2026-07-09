using System;
using System.Collections.Generic;
using Leopotam.EcsLite;

namespace OctoGames.TestTask.Gameplay.Units.Ecs.Systems
{
    public sealed class MoveUnitsSystem : UnitSystemBase, IEcsRunSystem
    {
        private readonly Queue<int> _freePoints = new();
        private EcsFilter _requestFilter;
        private EcsFilter _movingFilter;
        private int[] _entityByPoint = Array.Empty<int>();
        private bool[] _canMoveFromPoint = Array.Empty<bool>();

        public override void Init(IEcsSystems systems)
        {
            base.Init(systems);
            _requestFilter = World.Filter<MoveUnitsRequest>().End();
            _movingFilter = World.Filter<UnitMovingComponent>().End();
        }

        public void Run(IEcsSystems systems)
        {
            if (_requestFilter.GetEntitiesCount() == 0 || _movingFilter.GetEntitiesCount() > 0)
            {
                return;
            }

            BuildAndApplyMovePlan();
        }

        private void BuildAndApplyMovePlan()
        {
            int pointCount = Services.UnitGrid.Count;
            EnsureCapacity(pointCount);
            ClearBuffers(pointCount);

            foreach (int entity in UnitsFilter)
            {
                if (MovingPool.Has(entity))
                {
                    continue;
                }

                int pointIndex = GridPositionPool.Get(entity).CurrentPointIndex;
                if (pointIndex >= 0 && pointIndex < pointCount)
                {
                    _entityByPoint[pointIndex] = entity;
                }
            }

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                if (!Services.UnitGrid.IsReserved(pointIndex))
                {
                    _freePoints.Enqueue(pointIndex);
                }
            }

            while (_freePoints.Count > 0)
            {
                int freePointIndex = _freePoints.Dequeue();
                int previousPointIndex = GetPreviousPointIndex(freePointIndex, pointCount);
                if (_entityByPoint[previousPointIndex] < 0 ||
                    _canMoveFromPoint[previousPointIndex])
                {
                    continue;
                }

                _canMoveFromPoint[previousPointIndex] = true;
                _freePoints.Enqueue(previousPointIndex);
            }

            bool hasMoved = false;
            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                if (!_canMoveFromPoint[pointIndex])
                {
                    continue;
                }

                int entity = _entityByPoint[pointIndex];
                if (entity < 0)
                {
                    continue;
                }

                int targetPointIndex = GetNextPointIndex(pointIndex, pointCount);
                if (!Services.UnitGrid.AddReservation(targetPointIndex))
                {
                    continue;
                }

                ref UnitGridPositionComponent gridPosition = ref GridPositionPool.Get(entity);
                gridPosition.ReservedPointIndex = targetPointIndex;
                ref UnitMovingComponent moving = ref MovingPool.Add(entity);
                moving.TargetPosition = Services.UnitGrid.GetWorldPosition(targetPointIndex);
                hasMoved = true;
            }

            if (hasMoved)
            {
                Services.MarkUnitsSaveDirty();
            }
        }

        private void EnsureCapacity(int pointCount)
        {
            if (_entityByPoint.Length < pointCount)
            {
                _entityByPoint = new int[pointCount];
            }

            if (_canMoveFromPoint.Length < pointCount)
            {
                _canMoveFromPoint = new bool[pointCount];
            }
        }

        private void ClearBuffers(int pointCount)
        {
            for (int i = 0; i < pointCount; i++)
            {
                _entityByPoint[i] = -1;
                _canMoveFromPoint[i] = false;
            }

            _freePoints.Clear();
        }

        private static int GetNextPointIndex(int pointIndex, int pointCount)
        {
            return (pointIndex + 1) % pointCount;
        }

        private static int GetPreviousPointIndex(int pointIndex, int pointCount)
        {
            return pointIndex <= 0 ? pointCount - 1 : pointIndex - 1;
        }
    }
}
