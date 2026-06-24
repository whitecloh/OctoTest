using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Entities
{
    public sealed class GameplayEntity : MonoBehaviour, IGameplayEntity
    {
        private bool _isRemoved;

        public bool IsActive => !_isRemoved && isActiveAndEnabled && gameObject.activeInHierarchy;

        public void MarkRemoved()
        {
            if (_isRemoved)
            {
                return;
            }

            _isRemoved = true;
            GameplayEntityRegistry.Unregister(this);
        }

        private void OnEnable()
        {
            if (!_isRemoved)
            {
                GameplayEntityRegistry.Register(this);
            }
        }

        private void OnDisable()
        {
            GameplayEntityRegistry.Unregister(this);
        }

        private void OnDestroy()
        {
            GameplayEntityRegistry.Unregister(this);
        }
    }
}
