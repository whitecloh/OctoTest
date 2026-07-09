using System.Collections.Generic;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units.Data
{
    [CreateAssetMenu(menuName = "Octo Games/Test Task/Unit Catalog")]
    public sealed class UnitCatalog : ScriptableObject
    {
        [SerializeField] private List<UnitDefinition> units = new ();

        public IReadOnlyList<UnitDefinition> Units => units;
        public UnitDefinition FirstValid => GetFirstValid();

        public UnitDefinition GetRandom()
        {
            int count = units.Count;
            if (count == 0)
            {
                return new UnitDefinition();
            }

            int startIndex = Random.Range(0, count);
            for (int offset = 0; offset < count; offset++)
            {
                UnitDefinition unit = units[(startIndex + offset) % count];
                if (unit != null)
                {
                    return unit;
                }
            }

            return new UnitDefinition();
        }

        public UnitDefinition GetByIdOrDefault(string id)
        {
            return TryGet(id, out UnitDefinition unit) ? unit : FirstValid;
        }

        public bool Contains(string id)
        {
            return TryGet(id, out _);
        }

        public bool TryGet(string id, out UnitDefinition unit)
        {
            unit = null;
            if (string.IsNullOrWhiteSpace(id) || units == null)
            {
                return false;
            }

            string normalizedId = id.Trim();
            for (int i = 0; i < units.Count; i++)
            {
                UnitDefinition candidate = units[i];
                if (candidate != null && candidate.Id == normalizedId)
                {
                    unit = candidate;
                    return true;
                }
            }

            return false;
        }

        private UnitDefinition GetFirstValid()
        {
            if (units == null)
            {
                return null;
            }

            for (int i = 0; i < units.Count; i++)
            {
                if (units[i] != null)
                {
                    return units[i];
                }
            }

            return null;
        }
    }
}
