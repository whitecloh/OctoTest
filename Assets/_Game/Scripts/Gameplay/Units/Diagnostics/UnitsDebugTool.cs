using System.Collections;
using OctoGames.TestTask.Data;
using OctoGames.TestTask.Gameplay.Units.Presentation;
using OctoGames.TestTask.Gameplay.Units.Runtime;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units.Diagnostics
{
    public sealed class UnitsDebugTool : MonoBehaviour
    {
        [Header("Auto Run")]
        [SerializeField] private bool runStressOnStart;
        [SerializeField] private bool runThrottleProbeOnStart;

        [Header("Limits")]
        [SerializeField, Min(1)] private int stressSpawnAttempts = 64;
        [SerializeField, Min(1)] private int stressMoveCommands = 8;
        [SerializeField, Min(1)] private int throttleProbeChanges = 25;
        [SerializeField, Min(0f)] private float commandDelay = 0.02f;

        private UnitCommands _unitCommands;
        private UnitQuery _unitQuery;
        private UnitStatsService _statsService;
        private UnitsDashboardPresenter _presenter;
        private GameConfig _settings;
        private Coroutine _runningTest;

        public void Initialize(UnitCommands unitCommands, UnitQuery unitQuery, UnitStatsService statsService, UnitsDashboardPresenter presenter, GameConfig settings)
        {
            _unitCommands = unitCommands;
            _unitQuery = unitQuery;
            _statsService = statsService;
            _presenter = presenter;
            _settings = settings;

            if (runStressOnStart)
            {
                RunStressTest();
            }

            if (runThrottleProbeOnStart)
            {
                RunThrottleProbe();
            }
        }

        [ContextMenu("Run Safe Stress Test")]
        public void RunStressTest()
        {
            StartDebugTest(StressRoutine());
        }

        [ContextMenu("Run Throttling Probe")]
        public void RunThrottleProbe()
        {
            StartDebugTest(ThrottleProbeRoutine());
        }

        private void StartDebugTest(IEnumerator routine)
        {
            if (!Application.isPlaying || _unitCommands == null || routine == null)
            {
                return;
            }

            if (_runningTest != null)
            {
                StopCoroutine(_runningTest);
            }

            _runningTest = StartCoroutine(routine);
        }

        private IEnumerator StressRoutine()
        {
            int spawnLimit = Mathf.Min(stressSpawnAttempts, _settings != null ? _settings.MaxUnits : stressSpawnAttempts);
            _unitCommands.StartNewGame();

            for (int i = 0; i < spawnLimit; i++)
            {
                _unitCommands.SpawnUnit();
                yield return WaitCommandDelay();
            }

            for (int i = 0; i < stressMoveCommands; i++)
            {
                _unitCommands.IncreaseRandomValue();
                if (_unitQuery is { HasMovingUnits: false })
                {
                    _unitCommands.MoveUnits();
                }

                yield return null;
                yield return WaitUntilMovementEnds();
            }

            LogStats("Stress test finished");
            _runningTest = null;
        }

        private IEnumerator ThrottleProbeRoutine()
        {
            int refreshCountBefore = _presenter != null ? _presenter.RefreshCount : 0;
            for (int i = 0; i < throttleProbeChanges; i++)
            {
                _unitCommands.IncreaseRandomValue();
            }

            float waitTime = _settings != null ? _settings.UnitsViewUpdateInterval + 0.1f : 0.35f;
            yield return new WaitForSeconds(waitTime);

            int refreshCountAfter = _presenter != null ? _presenter.RefreshCount : 0;
            Debug.Log($"{nameof(UnitsDebugTool)} throttle probe finished. Refresh delta: {refreshCountAfter - refreshCountBefore}, changes: {throttleProbeChanges}.");
            _runningTest = null;
        }

        private IEnumerator WaitUntilMovementEnds()
        {
            float timeout = 5f;
            while (_unitQuery is { HasMovingUnits: true } && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            yield return WaitCommandDelay();
        }

        private object WaitCommandDelay()
        {
            return commandDelay <= 0f ? null : new WaitForSeconds(commandDelay);
        }

        private void LogStats(string prefix)
        {
            if (_statsService == null || _settings == null)
            {
                return;
            }

            UnitStats stats = _statsService.Calculate(_unitQuery, _settings.MaxUnits);
            Debug.Log($"{nameof(UnitsDebugTool)} {prefix}. Units: {stats.TotalCount}/{stats.MaxCount}, value: {stats.TotalValue}, avg: {stats.AverageValue:0.##}.");
        }
    }
}
