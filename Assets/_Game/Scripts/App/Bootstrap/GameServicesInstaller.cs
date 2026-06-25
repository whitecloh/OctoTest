using System;
using System.IO;
using OctoGames.TestTask.Core.SaveLoad;
using OctoGames.TestTask.Data;
using OctoGames.TestTask.Gameplay.Units;
using OctoGames.TestTask.UI.Popups;
using UnityEngine;

namespace OctoGames.TestTask.App.Bootstrap
{
    public sealed class GameServicesInstaller : MonoBehaviour
    {
        [SerializeField] private OctoTestSettings settings;
        [SerializeField] private UnitCatalog unitCatalog;

        public GameContext Build()
        {
            if (settings == null || unitCatalog == null)
            {
                throw new InvalidOperationException($"{nameof(GameServicesInstaller)} requires assigned {nameof(OctoTestSettings)} and {nameof(UnitCatalog)}.");
            }

            string saveDirectory = Path.Combine(Application.persistentDataPath, settings.SaveDirectoryName);
            
            SaveLoadService saveLoadService = new SaveLoadService(new JsonSaveSerializer(), saveDirectory);
            PopupService popupService = new PopupService();
            UnitService unitService = new UnitService(settings, saveLoadService, unitCatalog);
            UnitStatsService unitStatsService = new UnitStatsService();

            ServiceRegistry serviceRegistry = new ServiceRegistry();
            serviceRegistry.Bind(settings);
            serviceRegistry.Bind(unitCatalog);
            serviceRegistry.Bind<ISaveLoadService>(saveLoadService);
            serviceRegistry.Bind(saveLoadService);
            serviceRegistry.Bind(popupService);
            serviceRegistry.Bind(unitService);
            serviceRegistry.Bind(unitStatsService);

            return new GameContext(serviceRegistry);
        }
    }
}
