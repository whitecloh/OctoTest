using Leopotam.EcsLite;

namespace OctoGames.TestTask.Gameplay.Units.Ecs.Systems
{
    public sealed class SpawnUnitSystem : UnitSystemBase, IEcsRunSystem
    {
        private EcsFilter _requestFilter;

        public override void Init(IEcsSystems systems)
        {
            base.Init(systems);
            _requestFilter = World.Filter<SpawnUnitRequest>().End();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int _ in _requestFilter)
            {
                if (TrySpawnRandomUnit())
                {
                    Services.MarkUnitsSaveDirty();
                }
            }
        }
    }
}
