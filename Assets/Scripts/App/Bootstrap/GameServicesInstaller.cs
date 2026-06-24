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

        public GameContext Build()
        {
            if (settings == null)
            {
                throw new InvalidOperationException($"{nameof(GameServicesInstaller)} requires assigned {nameof(OctoTestSettings)}.");
            }

            string saveDirectory = Path.Combine(Application.persistentDataPath, settings.SaveDirectoryName);
            
            SaveLoadService saveLoadService = new SaveLoadService(new JsonSaveSerializer(), saveDirectory);
            PopupService popupService = new PopupService();
            UnitService unitService = new UnitService(settings, saveLoadService);
            UnitStatsService unitStatsService = new UnitStatsService();

            return new GameContext(settings, saveLoadService, popupService, unitService, unitStatsService);
        }
    }
}
