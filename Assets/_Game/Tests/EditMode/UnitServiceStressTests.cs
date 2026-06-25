using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using OctoGames.TestTask.Core.SaveLoad;
using OctoGames.TestTask.Data;
using OctoGames.TestTask.Gameplay.Units;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OctoGames.TestTask.Tests
{
    public sealed class UnitServiceStressTests
    {
        private string _saveDirectory;
        private OctoTestSettings _settings;
        private UnitCatalog _catalog;
        private UnitService _service;
        private SaveLoadService _saveLoadService;

        [SetUp]
        public void SetUp()
        {
            _saveDirectory = Path.Combine(Path.GetTempPath(), $"{nameof(UnitServiceStressTests)}_{Guid.NewGuid():N}");
            _settings = TestAssetFactory.CreateSettings("stress_units", 8, 8, 6, 48);
            _catalog = TestAssetFactory.CreateCatalog();
            _saveLoadService = new SaveLoadService(new JsonSaveSerializer(), _saveDirectory);
            _service = new UnitService(_settings, _saveLoadService, _catalog);
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();

            if (_settings != null)
            {
                Object.DestroyImmediate(_settings);
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
            Assert.DoesNotThrow(() =>
            {
                _service.StartNewGame();

                for (int i = 0; i < 96; i++)
                {
                    _service.SpawnUnit();
                }

                for (int i = 0; i < 32; i++)
                {
                    _service.IncreaseRandomValue();
                    _service.MoveUnits();
                    CompleteMovingUnits();
                }
            });

            Assert.LessOrEqual(_service.Units.Count, _settings.MaxUnits);
            AssertUniqueOccupiedPoints();
            AssertSavedDataIsValid();
        }

        private void CompleteMovingUnits()
        {
            for (int i = 0; i < _service.Units.Count; i++)
            {
                UnitRuntimeState unit = _service.Units[i];
                if (unit != null && unit.IsMoving)
                {
                    _service.CompleteMove(unit.RuntimeId);
                }
            }
        }

        private void AssertUniqueOccupiedPoints()
        {
            HashSet<int> points = new HashSet<int>();
            for (int i = 0; i < _service.Units.Count; i++)
            {
                UnitRuntimeState unit = _service.Units[i];
                Assert.NotNull(unit);
                Assert.IsTrue(points.Add(unit.CurrentPointIndex), $"Duplicate occupied point: {unit.CurrentPointIndex}");
                Assert.IsTrue(_catalog.Contains(unit.DataId), $"Unknown unit data id: {unit.DataId}");
            }
        }

        private void AssertSavedDataIsValid()
        {
            SaveLoadResult<UnitsSaveData> result = _saveLoadService.Load<UnitsSaveData>(_settings.UnitsSaveFileName);
            Assert.IsTrue(result.Success, result.Error);
            Assert.NotNull(result.Data);
            Assert.AreEqual(UnitsSaveData_v_1_0.CurrentVersion, result.Data.SaveVersion);
            Assert.LessOrEqual(result.Data.Units.Count, _settings.MaxUnits);

            for (int i = 0; i < result.Data.Units.Count; i++)
            {
                UnitSaveData savedUnit = result.Data.Units[i];
                Assert.NotNull(savedUnit);
                Assert.IsTrue(_catalog.Contains(savedUnit.DataId), $"Unknown saved data id: {savedUnit.DataId}");
                Assert.GreaterOrEqual(savedUnit.PointIndex, 0);
                Assert.Less(savedUnit.PointIndex, _settings.SpawnPointCount);
            }
        }
    }
}
