using System;
using System.Collections.Generic;
using Leopotam.EcsLite;
using OctoGames.TestTask.Gameplay.Units.Ecs;

namespace OctoGames.TestTask.Gameplay.Units.Runtime
{
    public sealed class UnitQuery
    {
        private readonly EcsFilter _unitsFilter;
        private readonly EcsFilter _movingUnitsFilter;
        private readonly EcsPool<UnitComponent> _unitPool;
        private readonly EcsPool<UnitValueComponent> _valuePool;
        private readonly EcsPool<UnitGridPositionComponent> _gridPool;
        private readonly EcsPool<UnitWorldPositionComponent> _positionPool;
        private readonly EcsPool<UnitMovingComponent> _movingPool;

        public UnitQuery(EcsWorld world)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            _unitsFilter = world
                .Filter<UnitComponent>()
                .Inc<UnitValueComponent>()
                .Inc<UnitGridPositionComponent>()
                .Inc<UnitWorldPositionComponent>()
                .End();
            _movingUnitsFilter = world.Filter<UnitMovingComponent>().End();
            _unitPool = world.GetPool<UnitComponent>();
            _valuePool = world.GetPool<UnitValueComponent>();
            _gridPool = world.GetPool<UnitGridPositionComponent>();
            _positionPool = world.GetPool<UnitWorldPositionComponent>();
            _movingPool = world.GetPool<UnitMovingComponent>();
        }

        public bool HasMovingUnits => _movingUnitsFilter.GetEntitiesCount() > 0;

        public int Count => _unitsFilter.GetEntitiesCount();

        public void Fill(List<UnitSnapshot> result)
        {
            if (result == null)
            {
                return;
            }

            result.Clear();
            foreach (int entity in _unitsFilter)
            {
                UnitComponent unit = _unitPool.Get(entity);
                UnitValueComponent value = _valuePool.Get(entity);
                UnitGridPositionComponent grid = _gridPool.Get(entity);
                UnitWorldPositionComponent position = _positionPool.Get(entity);
                result.Add(new UnitSnapshot(
                    unit.RuntimeId,
                    unit.DataId,
                    value.Value,
                    grid.CurrentPointIndex,
                    grid.ReservedPointIndex,
                    _movingPool.Has(entity),
                    position.Position));
            }
        }

        public UnitStats CalculateStats(int maxUnits)
        {
            int count = 0;
            int totalValue = 0;
            foreach (int entity in _unitsFilter)
            {
                count++;
                totalValue += _valuePool.Get(entity).Value;
            }

            return new UnitStats(count, maxUnits, totalValue);
        }
    }
}
