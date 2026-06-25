using System.Collections.Generic;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Entities
{
    public sealed class GameplayEntityRegistry
    {
        private readonly HashSet<IGameplayEntity> _entities = new ();
        private readonly List<IGameplayEntity> _invalidEntities = new ();

        public int Count => _entities.Count;

        public void Register(IGameplayEntity entity)
        {
            if (!IsEntityReferenceValid(entity) || !entity.IsActive)
            {
                return;
            }

            _entities.Add(entity);
        }

        public void Unregister(IGameplayEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            _entities.Remove(entity);
        }

        public void GetActiveEntities(List<IGameplayEntity> result)
        {
            if (result == null)
            {
                return;
            }

            result.Clear();
            _invalidEntities.Clear();

            foreach (IGameplayEntity entity in _entities)
            {
                if (!IsEntityReferenceValid(entity) || !entity.IsActive)
                {
                    _invalidEntities.Add(entity);
                    continue;
                }

                result.Add(entity);
            }

            for (int i = 0; i < _invalidEntities.Count; i++)
            {
                _entities.Remove(_invalidEntities[i]);
            }

            _invalidEntities.Clear();
        }

        public void Clear()
        {
            _entities.Clear();
            _invalidEntities.Clear();
        }

        private static bool IsEntityReferenceValid(IGameplayEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            if (entity is Object unityObject && unityObject == null)
            {
                return false;
            }

            return true;
        }
    }
}
