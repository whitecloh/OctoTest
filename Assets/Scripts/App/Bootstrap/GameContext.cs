using System;
using OctoGames.TestTask.Core.SaveLoad;
using OctoGames.TestTask.Data;
using OctoGames.TestTask.Gameplay.Units;
using OctoGames.TestTask.UI.Popups;

namespace OctoGames.TestTask.App.Bootstrap
{
    public sealed class GameContext : IDisposable
    {
        public GameContext(OctoTestSettings settings, SaveLoadService saveLoadService, PopupService popupService, UnitService unitService, UnitStatsService unitStatsService)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            SaveLoadService = saveLoadService ?? throw new ArgumentNullException(nameof(saveLoadService));
            PopupService = popupService ?? throw new ArgumentNullException(nameof(popupService));
            UnitService = unitService ?? throw new ArgumentNullException(nameof(unitService));
            UnitStatsService = unitStatsService ?? throw new ArgumentNullException(nameof(unitStatsService));
        }

        public OctoTestSettings Settings { get; }
        public SaveLoadService SaveLoadService { get; }
        public PopupService PopupService { get; }
        public UnitService UnitService { get; }
        public UnitStatsService UnitStatsService { get; }

        public void Dispose()
        {
            UnitService.Dispose();
        }
    }
}
