using System.Collections.Generic;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Entities
{
    public static class GameplayEntityRegistry
    {
        private static readonly HashSet<IGameplayEntity> s_entities = new ();
        private static readonly List<IGameplayEntity> s_invalidEntities = new ();

        public static int Count => s_entities.Count;

        public static void Register(IGameplayEntity entity)
        {
            if (!IsEntityReferenceValid(entity) || !entity.IsActive)
            {
                return;
            }

            s_entities.Add(entity);
        }

        public static void Unregister(IGameplayEntity entity)
        {
            if (entity == null)
            {
                return;
            }

            s_entities.Remove(entity);
        }

        public static void GetActiveEntities(List<IGameplayEntity> result)
        {
            if (result == null)
            {
                return;
            }

            result.Clear();
            s_invalidEntities.Clear();

            foreach (IGameplayEntity entity in s_entities)
            {
                if (!IsEntityReferenceValid(entity) || !entity.IsActive)
                {
                    s_invalidEntities.Add(entity);
                    continue;
                }

                result.Add(entity);
            }

            for (int i = 0; i < s_invalidEntities.Count; i++)
            {
                s_entities.Remove(s_invalidEntities[i]);
            }

            s_invalidEntities.Clear();
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticState()
        {
            s_entities.Clear();
            s_invalidEntities.Clear();
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
