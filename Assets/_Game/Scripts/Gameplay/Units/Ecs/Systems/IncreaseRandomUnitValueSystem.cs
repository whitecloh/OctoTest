using Leopotam.EcsLite;
using Random = UnityEngine.Random;

namespace OctoGames.TestTask.Gameplay.Units.Ecs.Systems
{
    public sealed class IncreaseRandomUnitValueSystem : UnitSystemBase, IEcsRunSystem
    {
        private EcsFilter _requestFilter;

        public override void Init(IEcsSystems systems)
        {
            base.Init(systems);
            _requestFilter = World.Filter<IncreaseRandomUnitValueRequest>().End();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (int _ in _requestFilter)
            {
                if (TryIncreaseRandomValue())
                {
                    Services.MarkUnitsSaveDirty();
                }
            }
        }

        private bool TryIncreaseRandomValue()
        {
            int count = UnitsFilter.GetEntitiesCount();
            if (count == 0)
            {
                return false;
            }

            int targetIndex = Random.Range(0, count);
            int currentIndex = 0;
            foreach (int entity in UnitsFilter)
            {
                if (currentIndex++ != targetIndex)
                {
                    continue;
                }

                ref UnitValueComponent value = ref ValuePool.Get(entity);
                value.Value += Services.Config.ValueIncreaseAmount;
                return true;
            }

            return false;
        }
    }
}
