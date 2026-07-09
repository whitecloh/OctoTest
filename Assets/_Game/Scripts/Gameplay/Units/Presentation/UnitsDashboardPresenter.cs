using System;
using OctoGames.TestTask.Data;
using OctoGames.TestTask.Gameplay.Units.Runtime;
using OctoGames.TestTask.UI.Dashboard;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units.Presentation
{
    public sealed class UnitsDashboardPresenter : MonoBehaviour, IDisposable
    {
        private UnitsStatsView _view;
        private UnitsDashboardView _dashboardView;
        private UnitCommands _unitCommands;
        private UnitQuery _unitQuery;
        private UnitStatsService _statsService;
        private GameConfig _settings;
        private readonly RefreshThrottle _refreshThrottle = new ();
        private string _labelFormat;

        public int RefreshCount { get; private set; }

        public void Initialize(
            UnitsDashboardView dashboardView,
            UnitCommands unitCommands,
            UnitQuery unitQuery,
            UnitStatsService stats,
            GameConfig projectSettings)
        {
            Dispose();

            _dashboardView = dashboardView ?? throw new ArgumentNullException(nameof(dashboardView));
            _unitCommands = unitCommands ?? throw new ArgumentNullException(nameof(unitCommands));
            _unitQuery = unitQuery ?? throw new ArgumentNullException(nameof(unitQuery));
            _statsService = stats ?? throw new ArgumentNullException(nameof(stats));
            _settings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
            _labelFormat = _settings.UnitsLabelFormat;
            ShowDashboard();
            RefreshNow();
        }

        public void Dispose()
        {
            _view = null;
            _dashboardView = null;
            _unitCommands = null;
            _unitQuery = null;
            _statsService = null;
            _settings = null;
            _refreshThrottle.Reset();
            RefreshCount = 0;
        }

        public void Refresh()
        {
            if (_settings == null)
            {
                return;
            }

            _refreshThrottle.RequestRefresh(Time.time, _settings.UnitsViewUpdateInterval, RefreshNow);
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void ShowDashboard()
        {
            _dashboardView.Show("Unit Control", "Spawn, move, modify and remove units. State is loaded on boot and saved after each command.", new[]
                {
                    new DashboardButtonData("Play", OnPlayClicked, false),
                    new DashboardButtonData("Spawn", OnSpawnClicked, false),
                    new DashboardButtonData("Move", OnMoveClicked, false),
                    new DashboardButtonData("Value +", OnValueClicked, false),
                    new DashboardButtonData("Remove", OnRemoveClicked, false)
                });

            if (_dashboardView.StatsView == null)
            {
                throw new InvalidOperationException($"{nameof(UnitsDashboardPresenter)} requires a {nameof(UnitsStatsView)} assigned on {nameof(UnitsDashboardView)}.");
            }

            _view = _dashboardView.StatsView;
        }

        private void RefreshNow()
        {
            if (_view == null || _unitQuery == null || _statsService == null || _settings == null)
            {
                return;
            }

            UnitStats stats = _statsService.Calculate(_unitQuery, _settings.MaxUnits);
            _view.SetStats(stats, _labelFormat);
            RefreshCount++;
        }

        private void OnPlayClicked()
        {
            _unitCommands?.StartNewGame();
        }

        private void OnSpawnClicked()
        {
            _unitCommands?.SpawnUnit();
        }

        private void OnMoveClicked()
        {
            if (_unitCommands == null || _unitQuery is { HasMovingUnits: true })
            {
                return;
            }

            _unitCommands.MoveUnits();
        }

        private void OnValueClicked()
        {
            _unitCommands?.IncreaseRandomValue();
        }

        private void OnRemoveClicked()
        {
            _unitCommands?.RemoveLast();
        }
    }
}
