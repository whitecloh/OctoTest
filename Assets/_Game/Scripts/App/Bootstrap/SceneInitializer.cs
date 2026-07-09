using System;
using OctoGames.TestTask.Gameplay.Units.Diagnostics;
using OctoGames.TestTask.Gameplay.Units.Presentation;
using OctoGames.TestTask.UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace OctoGames.TestTask.App.Bootstrap
{
    public sealed class SceneInitializer : MonoBehaviour, IDisposable
    {
        [Header("UI")]
        [SerializeField] private UiRoot uiRoot;

        [Header("Units")]
        [SerializeField] private UnitsScenePresenter unitsScenePresenter;
        [SerializeField] private UnitsDashboardPresenter unitsDashboardPresenter;
        [SerializeField] private UnitsDebugTool unitsDebugTool;

        private GameServices _services;

        public void Initialize(GameServices services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            ValidateRequiredReferences();

            uiRoot.ValidateRequiredReferences();
            uiRoot.HideAllWindows();

            unitsScenePresenter.Initialize(_services);
            unitsDashboardPresenter.Initialize(
                uiRoot.UnitsDashboardView,
                _services.UnitCommands,
                _services.UnitQuery,
                _services.UnitStatsService,
                _services.Config);

            unitsDebugTool?.Initialize(
                _services.UnitCommands,
                _services.UnitQuery,
                _services.UnitStatsService,
                unitsDashboardPresenter,
                _services.Config);
        }

        public void Refresh(GameServices services)
        {
            if (services == null)
            {
                return;
            }

            unitsScenePresenter?.Refresh();
            unitsDashboardPresenter?.Refresh();
        }

        public void Dispose()
        {
            unitsDashboardPresenter?.Dispose();
            unitsScenePresenter?.Dispose();
            _services = null;
        }

        private void ValidateRequiredReferences()
        {
            if (uiRoot == null || unitsScenePresenter == null || unitsDashboardPresenter == null)
            {
                throw new InvalidOperationException($"{nameof(SceneInitializer)} requires assigned scene references.");
            }
        }
    }
}
