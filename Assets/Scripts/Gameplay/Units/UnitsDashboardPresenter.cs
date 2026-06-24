using System;
using OctoGames.TestTask.Data;
using OctoGames.TestTask.UI.Popups;
using UnityEngine;
using OctoGames.TestTask.Views;

namespace OctoGames.TestTask.Gameplay.Units
{
    public sealed class UnitsDashboardPresenter : MonoBehaviour, IDisposable
    {
        private CharactersView _view;
        private PopupService _popupService;
        private UnitService _unitService;
        private UnitStatsService _statsService;
        private OctoTestSettings _settings;
        private string _labelFormat;
        private float _nextAllowedRefreshTime;
        private bool _isDirty;

        public void Initialize(PopupService service, UnitService units, UnitStatsService stats, OctoTestSettings projectSettings)
        {
            Dispose();

            _popupService = service ?? throw new ArgumentNullException(nameof(service));
            _unitService = units ?? throw new ArgumentNullException(nameof(units));
            _statsService = stats ?? throw new ArgumentNullException(nameof(stats));
            _settings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
            _labelFormat = _settings.UnitsLabelFormat;
            _unitService.Changed += OnUnitsChanged;
            ShowDashboard();
            RefreshNow();
        }

        public void Dispose()
        {
            if (_unitService != null)
            {
                _unitService.Changed -= OnUnitsChanged;
            }

            _view = null;
            _popupService = null;
            _unitService = null;
            _statsService = null;
            _settings = null;
            _isDirty = false;
        }

        private void Update()
        {
            if (!_isDirty || Time.time < _nextAllowedRefreshTime)
            {
                return;
            }

            RefreshNow();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        private void ShowDashboard()
        {
            PopupRequest request = new PopupRequest(
                "Unit Control",
                "Spawn, move, modify and remove units. State is loaded on boot and saved after each command.",
                new[]
                {
                    new PopupButtonData("Play", OnPlayClicked, false),
                    new PopupButtonData("Spawn", OnSpawnClicked, false),
                    new PopupButtonData("Move", OnMoveClicked, false),
                    new PopupButtonData("Value +", OnValueClicked, false),
                    new PopupButtonData("Remove", OnRemoveClicked, false)
                });
            
            DashboardPopupView popupView = _popupService.Show(request);
            
            if (popupView == null || popupView.CharactersView == null)
            {
                throw new InvalidOperationException($"{nameof(UnitsDashboardPresenter)} requires a {nameof(CharactersView)} assigned on {nameof(DashboardPopupView)}.");
            }

            _view = popupView.CharactersView;
        }

        private void OnUnitsChanged()
        {
            if (Time.time >= _nextAllowedRefreshTime)
            {
                RefreshNow();
                return;
            }

            _isDirty = true;
        }

        private void RefreshNow()
        {
            if (_view == null || _unitService == null || _statsService == null || _settings == null)
            {
                return;
            }

            UnitStats stats = _statsService.Calculate(_unitService.Units, _settings.MaxUnits);
            _view.SetStats(stats, _labelFormat);
            _nextAllowedRefreshTime = Time.time + _settings.UnitsViewUpdateInterval;
            _isDirty = false;
        }

        private void OnPlayClicked()
        {
            _unitService?.StartNewGame();
        }

        private void OnSpawnClicked()
        {
            _unitService?.SpawnUnit();
        }

        private void OnMoveClicked()
        {
            if (_unitService == null || _unitService.HasMovingUnits)
            {
                return;
            }

            _unitService.MoveUnits();
        }

        private void OnValueClicked()
        {
            _unitService?.IncreaseRandomValue();
        }

        private void OnRemoveClicked()
        {
            _unitService?.RemoveLast();
        }
    }
}
