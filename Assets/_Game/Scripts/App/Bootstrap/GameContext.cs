using System;
using OctoGames.TestTask.Core.SaveLoad;
using OctoGames.TestTask.Data;
using OctoGames.TestTask.Gameplay.Units;
using OctoGames.TestTask.UI.Popups;

namespace OctoGames.TestTask.App.Bootstrap
{
    public sealed class GameContext : IDisposable
    {
        private readonly ServiceRegistry _services;

        public GameContext(ServiceRegistry services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            Settings = _services.Resolve<OctoTestSettings>();
            SaveLoadService = _services.Resolve<ISaveLoadService>();
            PopupService = _services.Resolve<PopupService>();
            UnitCatalog = _services.Resolve<UnitCatalog>();
            UnitService = _services.Resolve<UnitService>();
            UnitStatsService = _services.Resolve<UnitStatsService>();
        }

        public OctoTestSettings Settings { get; }
        public ISaveLoadService SaveLoadService { get; }
        public PopupService PopupService { get; }
        public UnitCatalog UnitCatalog { get; }
        public UnitService UnitService { get; }
        public UnitStatsService UnitStatsService { get; }

        public void Dispose()
        {
            _services.Dispose();
        }
    }
}
