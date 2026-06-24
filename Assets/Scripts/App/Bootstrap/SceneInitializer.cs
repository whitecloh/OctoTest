using System;
using OctoGames.TestTask.Gameplay.Units;
using OctoGames.TestTask.UI.Popups;
using UnityEngine;

namespace OctoGames.TestTask.App.Bootstrap
{
    public sealed class SceneInitializer : MonoBehaviour, IDisposable
    {
        [Header("UI")]
        [SerializeField] private UiRoot uiRoot;

        [Header("Units")]
        [SerializeField] private UnitSceneService unitSceneService;
        [SerializeField] private UnitsDashboardPresenter unitsDashboardPresenter;

        private GameContext _context;

        public void Initialize(GameContext gameContext)
        {
            _context = gameContext ?? throw new ArgumentNullException(nameof(gameContext));
            ValidateRequiredReferences();

            uiRoot.ValidateRequiredReferences();
            _context.PopupService.Register(uiRoot.PopupViews);

            unitSceneService.Initialize(_context.UnitService, _context.Settings);
            unitsDashboardPresenter.Initialize(_context.PopupService, _context.UnitService, _context.UnitStatsService, _context.Settings);

            _context.UnitService.LoadOrCreateInitial();
        }

        public void Dispose()
        {
            unitsDashboardPresenter?.Dispose();
            unitSceneService?.Dispose();
            _context?.PopupService?.UnregisterAll();
            _context = null;
        }

        private void ValidateRequiredReferences()
        {
            if (uiRoot == null || unitSceneService == null || unitsDashboardPresenter == null)
            {
                throw new InvalidOperationException($"{nameof(SceneInitializer)} requires assigned scene references.");
            }
        }
    }
}
