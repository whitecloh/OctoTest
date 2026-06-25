using System.Collections.Generic;
using System.Reflection;
using OctoGames.TestTask.Data;
using OctoGames.TestTask.Gameplay.Units;
using UnityEngine;

namespace OctoGames.TestTask.Tests
{
    internal static class TestAssetFactory
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.NonPublic;

        public static OctoTestSettings CreateSettings(string saveFileName, int columns, int rows, int initialUnits, int maxUnits)
        {
            OctoTestSettings settings = ScriptableObject.CreateInstance<OctoTestSettings>();
            Set(settings, "saveDirectoryName", "Tests");
            Set(settings, "unitsSaveFileName", saveFileName);
            Set(settings, "spawnZoneColumns", columns);
            Set(settings, "spawnZoneRows", rows);
            Set(settings, "spawnPointSpacing", 1f);
            Set(settings, "initialUnits", initialUnits);
            Set(settings, "maxUnits", maxUnits);
            Set(settings, "unitMoveSpeed", 20f);
            Set(settings, "valueIncreaseAmount", 1);
            Set(settings, "unitsViewUpdateInterval", 0.25f);
            return settings;
        }

        public static UnitCatalog CreateCatalog()
        {
            UnitCatalog catalog = ScriptableObject.CreateInstance<UnitCatalog>();
            Set(catalog, "units", new List<UnitDefinition>
            {
                CreateDefinition("test_red", Color.red, 1),
                CreateDefinition("test_green", Color.green, 2),
                CreateDefinition("test_blue", Color.blue, 3)
            });
            return catalog;
        }

        private static UnitDefinition CreateDefinition(string id, Color color, int startValue)
        {
            UnitDefinition definition = new UnitDefinition();
            Set(definition, "id", id);
            Set(definition, "color", color);
            Set(definition, "startValue", startValue);
            return definition;
        }

        private static void Set(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(fieldName, FieldFlags);
            field.SetValue(target, value);
        }
    }
}
