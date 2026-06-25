using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Entities
{
    public sealed class GameplayEntity : MonoBehaviour, IGameplayEntity
    {
        [SerializeField] private GameplayEntityRegistryScope registryScope;

        private GameplayEntityRegistry _registry;
        private bool _isRemoved;

        public bool IsActive => !_isRemoved && isActiveAndEnabled && gameObject.activeInHierarchy;

        public void MarkRemoved()
        {
            if (_isRemoved)
            {
                return;
            }

            _isRemoved = true;
            Unregister();
        }

        private void OnEnable()
        {
            if (!_isRemoved)
            {
                Register();
            }
        }

        private void OnDisable()
        {
            Unregister();
        }

        private void OnDestroy()
        {
            Unregister();
        }

        private void Register()
        {
            _registry = ResolveRegistry();
            _registry?.Register(this);
        }

        private void Unregister()
        {
            _registry?.Unregister(this);
            _registry = null;
        }

        private GameplayEntityRegistry ResolveRegistry()
        {
            if (registryScope == null)
            {
                registryScope = GetComponentInParent<GameplayEntityRegistryScope>();
            }

            return registryScope != null ? registryScope.Registry : null;
        }
    }
}
