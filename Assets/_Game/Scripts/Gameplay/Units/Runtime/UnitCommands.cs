using System;
using Leopotam.EcsLite;
using OctoGames.TestTask.Gameplay.Units.Ecs;

namespace OctoGames.TestTask.Gameplay.Units.Runtime
{
    public sealed class UnitCommands
    {
        private readonly EcsWorld _world;

        public UnitCommands(EcsWorld world)
        {
            _world = world ?? throw new ArgumentNullException(nameof(world));
        }

        public void LoadOrCreateInitial()
        {
            AddRequest<LoadOrCreateUnitsRequest>();
        }

        public void StartNewGame()
        {
            AddRequest<StartNewGameRequest>();
        }

        public void SpawnUnit()
        {
            AddRequest<SpawnUnitRequest>();
        }

        public void MoveUnits()
        {
            AddRequest<MoveUnitsRequest>();
        }

        public void IncreaseRandomValue()
        {
            AddRequest<IncreaseRandomUnitValueRequest>();
        }

        public void RemoveLast()
        {
            AddRequest<RemoveLastUnitRequest>();
        }

        private void AddRequest<TRequest>() where TRequest : struct
        {
            int entity = _world.NewEntity();
            _world.GetPool<TRequest>().Add(entity);
        }
    }
}
