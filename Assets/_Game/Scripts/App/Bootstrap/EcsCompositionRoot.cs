using Leopotam.EcsLite;
using OctoGames.TestTask.Gameplay.Units.Ecs.Systems;

namespace OctoGames.TestTask.App.Bootstrap
{
    public sealed class EcsCompositionRoot
    {
        private readonly GameServices _services;

        public EcsCompositionRoot(GameServices services)
        {
            _services = services;
        }

        public IEcsSystems Create(EcsWorld world)
        {
            return new EcsSystems(world, _services)
                .Add(new LoadOrStartUnitsSystem())
                .Add(new StartNewGameSystem())
                .Add(new SpawnUnitSystem())
                .Add(new MoveUnitsSystem())
                .Add(new UnitMovementSystem())
                .Add(new IncreaseRandomUnitValueSystem())
                .Add(new RemoveLastUnitSystem())
                .Add(new SaveUnitsSystem())
                .Add(new CleanupUnitRequestsSystem());
        }
    }
}
