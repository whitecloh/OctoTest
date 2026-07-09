using System;
using System.IO;
using Leopotam.EcsLite;
using OctoGames.TestTask.Core.SaveLoad;
using OctoGames.TestTask.Data;
using OctoGames.TestTask.Gameplay.Units.Data;
using OctoGames.TestTask.Gameplay.Units.Runtime;
using UnityEngine;

namespace OctoGames.TestTask.App.Bootstrap
{
    public sealed class GameServicesInstaller : MonoBehaviour
    {
        [SerializeField] private GameConfig settings;
        [SerializeField] private UnitCatalog unitCatalog;

        public GameServices Build(EcsWorld world)
        {
            if (world == null)
            {
                throw new ArgumentNullException(nameof(world));
            }

            if (settings == null || unitCatalog == null)
            {
                throw new InvalidOperationException($"{nameof(GameServicesInstaller)} requires assigned {nameof(GameConfig)} and {nameof(UnitCatalog)}.");
            }

            string saveDirectory = Path.Combine(Application.persistentDataPath, settings.SaveDirectoryName);
            
            SaveLoadService saveLoadService = new SaveLoadService(new JsonSaveSerializer(), saveDirectory);
            UnitSpawnGrid unitGrid = new UnitSpawnGrid(
                settings.SpawnZoneColumns,
                settings.SpawnZoneRows,
                settings.SpawnPointSpacing,
                Vector3.zero);
            UnitCommands unitCommands = new UnitCommands(world);
            UnitQuery unitQuery = new UnitQuery(world);
            UnitStatsService unitStatsService = new UnitStatsService();

            return new GameServices(
                settings,
                unitCatalog,
                saveLoadService,
                unitGrid,
                unitCommands,
                unitQuery,
                unitStatsService);
        }
    }
}
