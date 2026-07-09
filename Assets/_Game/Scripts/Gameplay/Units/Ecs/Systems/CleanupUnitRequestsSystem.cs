using Leopotam.EcsLite;

namespace OctoGames.TestTask.Gameplay.Units.Ecs.Systems
{
    public sealed class CleanupUnitRequestsSystem : IEcsInitSystem, IEcsRunSystem
    {
        private EcsWorld _world;
        private EcsFilter _loadFilter;
        private EcsFilter _startFilter;
        private EcsFilter _spawnFilter;
        private EcsFilter _moveFilter;
        private EcsFilter _valueFilter;
        private EcsFilter _removeFilter;

        public void Init(IEcsSystems systems)
        {
            _world = systems.GetWorld();
            _loadFilter = _world.Filter<LoadOrCreateUnitsRequest>().End();
            _startFilter = _world.Filter<StartNewGameRequest>().End();
            _spawnFilter = _world.Filter<SpawnUnitRequest>().End();
            _moveFilter = _world.Filter<MoveUnitsRequest>().End();
            _valueFilter = _world.Filter<IncreaseRandomUnitValueRequest>().End();
            _removeFilter = _world.Filter<RemoveLastUnitRequest>().End();
        }

        public void Run(IEcsSystems systems)
        {
            DeleteAll(_loadFilter);
            DeleteAll(_startFilter);
            DeleteAll(_spawnFilter);
            DeleteAll(_moveFilter);
            DeleteAll(_valueFilter);
            DeleteAll(_removeFilter);
        }

        private void DeleteAll(EcsFilter filter)
        {
            foreach (int entity in filter)
            {
                _world.DelEntity(entity);
            }
        }
    }
}
