using Leopotam.EcsLite;

namespace OctoGames.TestTask.Gameplay.Units.Ecs.Systems
{
    public sealed class StartNewGameSystem : UnitSystemBase, IEcsRunSystem
    {
        private EcsFilter _requestFilter;

        public override void Init(IEcsSystems systems)
        {
            base.Init(systems);
            _requestFilter = World.Filter<StartNewGameRequest>().End();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int _ in _requestFilter)
            {
                StartNewGame();
                return;
            }
        }
    }
}
