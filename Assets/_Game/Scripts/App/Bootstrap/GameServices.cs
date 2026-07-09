using System;
using OctoGames.TestTask.Core.SaveLoad;
using OctoGames.TestTask.Data;
using OctoGames.TestTask.Gameplay.Units.Data;
using OctoGames.TestTask.Gameplay.Units.Runtime;

namespace OctoGames.TestTask.App.Bootstrap
{
    public sealed class GameServices
    {
        private bool _unitsSaveDirty;

        public GameServices(
            GameConfig config,
            UnitCatalog unitCatalog,
            ISaveLoadService saveLoadService,
            UnitSpawnGrid unitGrid,
            UnitCommands unitCommands,
            UnitQuery unitQuery,
            UnitStatsService unitStatsService)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            UnitCatalog = unitCatalog ?? throw new ArgumentNullException(nameof(unitCatalog));
            SaveLoadService = saveLoadService ?? throw new ArgumentNullException(nameof(saveLoadService));
            UnitGrid = unitGrid ?? throw new ArgumentNullException(nameof(unitGrid));
            UnitCommands = unitCommands ?? throw new ArgumentNullException(nameof(unitCommands));
            UnitQuery = unitQuery ?? throw new ArgumentNullException(nameof(unitQuery));
            UnitStatsService = unitStatsService ?? throw new ArgumentNullException(nameof(unitStatsService));
        }

        public GameConfig Config { get; }
        public UnitCatalog UnitCatalog { get; }
        public ISaveLoadService SaveLoadService { get; }
        public UnitSpawnGrid UnitGrid { get; }
        public UnitCommands UnitCommands { get; }
        public UnitQuery UnitQuery { get; }
        public UnitStatsService UnitStatsService { get; }
        public int NextUnitRuntimeId { get; set; } = 1;
        public float DeltaTime { get; private set; }

        public void SetDeltaTime(float deltaTime)
        {
            DeltaTime = Math.Max(0f, deltaTime);
        }

        public void MarkUnitsSaveDirty()
        {
            _unitsSaveDirty = true;
        }

        public bool ConsumeUnitsSaveDirty()
        {
            if (!_unitsSaveDirty)
            {
                return false;
            }

            _unitsSaveDirty = false;
            return true;
        }

    }
}
