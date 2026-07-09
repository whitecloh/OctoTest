using System;
using System.Collections.Generic;
using System.IO;
using Leopotam.EcsLite;
using NUnit.Framework;
using OctoGames.TestTask.App.Bootstrap;
using OctoGames.TestTask.Core.SaveLoad;
using OctoGames.TestTask.Data;
using OctoGames.TestTask.Gameplay.Units.Data;
using OctoGames.TestTask.Gameplay.Units.Persistence;
using OctoGames.TestTask.Gameplay.Units.Runtime;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OctoGames.TestTask.Tests
{
    public sealed class UnitEcsFlowTests
    {
        private string _saveDirectory;
        private GameConfig _config;
        private UnitCatalog _catalog;
        private EcsWorld _world;
        private IEcsSystems _systems;
        private GameServices _services;
        private SaveLoadService _saveLoadService;
        private readonly List<UnitSnapshot> _units = new();

        [SetUp]
        public void SetUp()
        {
            _saveDirectory = Path.Combine(Path.GetTempPath(), $"{nameof(UnitEcsFlowTests)}_{Guid.NewGuid():N}");
        }

        [TearDown]
        public void TearDown()
        {
            _systems?.Destroy();
            _world?.Destroy();
            if (_config != null)
            {
                Object.DestroyImmediate(_config);
            }

            if (_catalog != null)
            {
                Object.DestroyImmediate(_catalog);
            }

            if (!string.IsNullOrEmpty(_saveDirectory) && Directory.Exists(_saveDirectory))
            {
                Directory.Delete(_saveDirectory, true);
            }
        }

        [Test]
        public void StressCommands_StayWithinLimitsAndProduceValidSave()
        {
            CreateRuntime("stress_units", 8, 8, 6, 48);

            Assert.DoesNotThrow(() =>
            {
                _services.UnitCommands.StartNewGame();
                RunFrame();

                for (int i = 0; i < 96; i++)
                {
                    _services.UnitCommands.SpawnUnit();
                    RunFrame();
                }

                for (int i = 0; i < 32; i++)
                {
                    _services.UnitCommands.IncreaseRandomValue();
                    _services.UnitCommands.MoveUnits();
                    RunFrame();
                    CompleteMovingUnits();
                }
            });

            Assert.LessOrEqual(_services.UnitQuery.Count, _config.MaxUnits);
            AssertUniqueOccupiedPoints();
            AssertSavedDataIsValid();
        }

        [Test]
        public void SpawnRequests_StopAtConfiguredMaxUnits()
        {
            CreateRuntime("spawn_cap_units", 2, 2, 0, 3);

            for (int i = 0; i < 12; i++)
            {
                _services.UnitCommands.SpawnUnit();
                RunFrame();
            }

            Assert.AreEqual(3, _services.UnitQuery.Count);
            AssertUniqueOccupiedPoints();
        }

        [Test]
        public void MoveRequest_ShiftsUnitsIntoNextFreeCells()
        {
            CreateRuntime("move_units", 3, 1, 2, 3);
            _services.UnitCommands.StartNewGame();
            RunFrame();

            _services.UnitCommands.MoveUnits();
            RunFrame(1f);
            CompleteMovingUnits();

            _services.UnitQuery.Fill(_units);
            HashSet<int> occupiedPoints = new HashSet<int>();
            for (int i = 0; i < _units.Count; i++)
            {
                occupiedPoints.Add(_units[i].CurrentPointIndex);
            }

            Assert.AreEqual(2, occupiedPoints.Count);
            Assert.IsTrue(occupiedPoints.Contains(1));
            Assert.IsTrue(occupiedPoints.Contains(2));
        }

        [Test]
        public void LoadOrCreateInitial_RestoresSavedUnits()
        {
            CreateRuntime("restore_units", 3, 2, 0, 5);
            UnitsSaveData saveData = new UnitsSaveData { NextRuntimeId = 10 };
            saveData.AddUnit().Set(1, "test_red", 7, 0);
            saveData.AddUnit().Set(2, "test_green", 5, 4);
            Assert.IsTrue(_saveLoadService.Save(_config.UnitsSaveFileName, saveData));

            _services.UnitCommands.LoadOrCreateInitial();
            RunFrame();

            _services.UnitQuery.Fill(_units);
            Assert.AreEqual(2, _units.Count);
            AssertUnitRestored("test_red", 7, 0);
            AssertUnitRestored("test_green", 5, 4);
        }

        private void CreateRuntime(string saveFileName, int columns, int rows, int initialUnits, int maxUnits)
        {
            _config = TestAssetFactory.CreateSettings(saveFileName, columns, rows, initialUnits, maxUnits);
            _catalog = TestAssetFactory.CreateCatalog();
            _saveLoadService = new SaveLoadService(new JsonSaveSerializer(), _saveDirectory);
            _world = new EcsWorld();
            _services = new GameServices(
                _config,
                _catalog,
                _saveLoadService,
                new UnitSpawnGrid(_config.SpawnZoneColumns, _config.SpawnZoneRows, _config.SpawnPointSpacing, Vector3.zero),
                new UnitCommands(_world),
                new UnitQuery(_world),
                new UnitStatsService());
            _systems = new EcsCompositionRoot(_services).Create(_world);
            _systems.Init();
        }

        private void CompleteMovingUnits()
        {
            int guard = 128;
            while (_services.UnitQuery.HasMovingUnits && guard-- > 0)
            {
                RunFrame(1f);
            }
        }

        private void AssertUniqueOccupiedPoints()
        {
            HashSet<int> points = new HashSet<int>();
            _services.UnitQuery.Fill(_units);
            for (int i = 0; i < _units.Count; i++)
            {
                UnitSnapshot unit = _units[i];
                Assert.IsTrue(points.Add(unit.CurrentPointIndex), $"Duplicate occupied point: {unit.CurrentPointIndex}");
                Assert.IsTrue(_catalog.Contains(unit.DataId), $"Unknown unit data id: {unit.DataId}");
            }
        }

        private void AssertSavedDataIsValid()
        {
            SaveLoadResult<UnitsSaveData> result = _saveLoadService.Load<UnitsSaveData>(_config.UnitsSaveFileName);
            Assert.IsTrue(result.Success, result.Error);
            Assert.NotNull(result.Data);
            Assert.AreEqual(UnitsSaveData_v_1_0.CurrentVersion, result.Data.SaveVersion);
            Assert.LessOrEqual(result.Data.Units.Count, _config.MaxUnits);

            for (int i = 0; i < result.Data.Units.Count; i++)
            {
                UnitSaveData savedUnit = result.Data.Units[i];
                Assert.NotNull(savedUnit);
                Assert.IsTrue(_catalog.Contains(savedUnit.DataId), $"Unknown saved data id: {savedUnit.DataId}");
                Assert.GreaterOrEqual(savedUnit.PointIndex, 0);
                Assert.Less(savedUnit.PointIndex, _config.SpawnPointCount);
            }
        }

        private void AssertUnitRestored(string dataId, int value, int pointIndex)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                UnitSnapshot unit = _units[i];
                if (unit.DataId == dataId &&
                    unit.Value == value &&
                    unit.CurrentPointIndex == pointIndex)
                {
                    return;
                }
            }

            Assert.Fail($"Expected restored unit '{dataId}' value {value} at point {pointIndex}.");
        }

        private void RunFrame(float deltaTime = 0.1f)
        {
            _services.SetDeltaTime(deltaTime);
            _systems.Run();
        }
    }
}
