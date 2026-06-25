using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Entities
{
    public sealed class GameplayEntityRegistryScope : MonoBehaviour
    {
        private readonly GameplayEntityRegistry _registry = new ();

        public GameplayEntityRegistry Registry => _registry;

        private void OnDestroy()
        {
            _registry.Clear();
        }
    }
}
