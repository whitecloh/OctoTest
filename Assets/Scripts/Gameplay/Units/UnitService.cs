using System;
using System.Collections.Generic;
using OctoGames.TestTask.Core.SaveLoad;
using OctoGames.TestTask.Data;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units
{
    public sealed class UnitService : IDisposable
    {
        private readonly OctoTestSettings _settings;
        private readonly SaveLoadService _saveLoadService;
        private readonly List<UnitRuntimeState> _units = new ();
        private readonly List<UnitMovePlan> _movePlans = new ();
        private readonly UnitSpawnGrid _grid;
        private int _nextRuntimeId = 1;

        public UnitService(OctoTestSettings settings, SaveLoadService saveLoadService)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _saveLoadService = saveLoadService ?? throw new ArgumentNullException(nameof(saveLoadService));
            _grid = new UnitSpawnGrid(settings.SpawnZoneColumns, settings.SpawnZoneRows, settings.SpawnPointSpacing, Vector3.zero);
        }

        public event Action Changed;
        public event Action<UnitRuntimeState> UnitAdded;
        public event Action<UnitRuntimeState> UnitChanged;
        public event Action<int> UnitRemoved;
        public event Action Reset;

        public IReadOnlyList<UnitRuntimeState> Units => _units;
        public UnitSpawnGrid Grid => _grid;
        public bool HasMovingUnits => HasAnyMovingUnit();

        public void SetGridOrigin(Vector3 origin)
        {
            _grid.SetOrigin(origin);
        }

        public void LoadOrCreateInitial()
        {
            SaveLoadResult<UnitsSaveData> result = _saveLoadService.Load<UnitsSaveData>(_settings.UnitsSaveFileName);
            
            if (result is { Success: true, Data: not null })
            {
                ApplySave(result.Data);
                return;
            }

            StartNewGame();
        }

        public void StartNewGame()
        {
            Clear();
            _nextRuntimeId = 1;

            for (int i = 0; i < _settings.InitialUnits; i++)
            {
                SpawnUnitInternal(false);
            }

            Save();
            NotifyChanged();
        }

        public bool SpawnUnit()
        {
            return SpawnUnitInternal(true);
        }
        
        private bool HasAnyMovingUnit()
        {
            for (int i = 0; i < _units.Count; i++)
            {
                UnitRuntimeState unit = _units[i];
                if (unit is { IsMoving: true })
                {
                    return true;
                }
            }

            return false;
        }
        
        private bool SpawnUnitInternal(bool saveAndNotify)
        {
            if (_units.Count >= _settings.MaxUnits || !_grid.TryReserveFirstFree(out int pointIndex))
            {
                return false;
            }

            int runtimeId = _nextRuntimeId++;
            string dataId = _settings.UnitDataId;
            int value = _settings.DefaultUnitInitialValue;
            Vector3 position = _grid.GetWorldPosition(pointIndex);

            UnitRuntimeState unit = new UnitRuntimeState(
                runtimeId,
                dataId,
                value,
                pointIndex,
                pointIndex,
                false,
                position);

            unit.Changed += OnUnitChanged;
            _units.Add(unit);
            UnitAdded?.Invoke(unit);

            if (saveAndNotify)
            {
                Save();
                NotifyChanged();
            }

            return true;
        }

        public void MoveUnits()
        {
            if (HasAnyMovingUnit())
            {
                return;
            }
            
            UnitMovementPlanner.BuildMovePlan(_units, _grid, _movePlans);
            bool hasMoved = false;
            for (int i = 0; i < _movePlans.Count; i++)
            {
                UnitMovePlan movePlan = _movePlans[i];
                if (movePlan.Unit == null || !_grid.AddReservation(movePlan.TargetPointIndex))
                {
                    continue;
                }

                movePlan.Unit.BeginMove(movePlan.TargetPointIndex, movePlan.Unit.Position);
                hasMoved = true;
            }

            if (!hasMoved)
            {
                return;
            }

            Save();
            NotifyChanged();
        }

        public void CompleteMove(int runtimeId)
        {
            UnitRuntimeState unit = Find(runtimeId);
            if (unit == null || !unit.IsMoving)
            {
                return;
            }

            int previousPointIndex = unit.CurrentPointIndex;
            unit.CompleteMove(_grid.GetWorldPosition(unit.ReservedPointIndex));
            if (previousPointIndex != unit.ReservedPointIndex)
            {
                _grid.Release(previousPointIndex);
            }
        }

        public void UpdatePosition(int runtimeId, Vector3 position)
        {
            Find(runtimeId)?.SetPosition(position);
        }

        public void IncreaseRandomValue()
        {
            UnitRuntimeState unit = FindRandom();
            if (unit == null)
            {
                return;
            }

            unit.SetValue(unit.Value + _settings.ValueIncreaseAmount);
            Save();
            NotifyChanged();
        }

        public void RemoveLast()
        {
            for (int i = _units.Count - 1; i >= 0; i--)
            {
                UnitRuntimeState unit = _units[i];
                if (unit != null)
                {
                    Remove(unit);
                    return;
                }
            }
        }

        private bool Save()
        {
            UnitsSaveData saveData = UnitSaveMapper.ToSaveData(_units, _nextRuntimeId);
            bool saved = _saveLoadService.Save(_settings.UnitsSaveFileName, saveData);
            if (!saved)
            {
                Debug.LogWarning($"{nameof(UnitService)} failed to save units state.");
            }

            return saved;
        }

        public void Dispose()
        {
            Clear();
            Changed = null;
            UnitAdded = null;
            UnitChanged = null;
            UnitRemoved = null;
            Reset = null;
        }

        private void ApplySave(UnitsSaveData saveData)
        {
            Clear();
            UnitSaveMapper.RestoreUnits(saveData, _grid, _settings.MaxUnits, _settings.UnitDataId, _units, ref _nextRuntimeId);
            
            for (int i = 0; i < _units.Count; i++)
            {
                UnitRuntimeState unit = _units[i];
                unit.Changed += OnUnitChanged;
                UnitAdded?.Invoke(unit);
            }

            NotifyChanged();
            Save();
        }

        private UnitRuntimeState Find(int runtimeId)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                UnitRuntimeState unit = _units[i];
                if (unit != null && unit.RuntimeId == runtimeId)
                {
                    return unit;
                }
            }

            return null;
        }

        private UnitRuntimeState FindRandom()
        {
            if (_units.Count == 0)
            {
                return null;
            }

            int startIndex = UnityEngine.Random.Range(0, _units.Count);
            for (int offset = 0; offset < _units.Count; offset++)
            {
                int index = (startIndex + offset) % _units.Count;
                UnitRuntimeState unit = _units[index];
                if (unit != null)
                {
                    return unit;
                }
            }

            return null;
        }

        private void Remove(UnitRuntimeState unit)
        {
            unit.Changed -= OnUnitChanged;
            _grid.Release(unit.CurrentPointIndex);
            if (unit.ReservedPointIndex != unit.CurrentPointIndex)
            {
                _grid.Release(unit.ReservedPointIndex);
            }

            _units.Remove(unit);
            UnitRemoved?.Invoke(unit.RuntimeId);
            Save();
            NotifyChanged();
        }

        private void Clear()
        {
            for (int i = 0; i < _units.Count; i++)
            {
                UnitRuntimeState unit = _units[i];
                if (unit != null)
                {
                    unit.Changed -= OnUnitChanged;
                }
            }

            _units.Clear();
            _grid.Clear();
            Reset?.Invoke();
        }

        private void OnUnitChanged(UnitRuntimeState unit)
        {
            UnitChanged?.Invoke(unit);
        }

        private void NotifyChanged()
        {
            Changed?.Invoke();
        }
    }
}
