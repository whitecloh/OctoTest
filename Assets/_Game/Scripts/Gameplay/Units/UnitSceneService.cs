using System;
using System.Collections.Generic;
using OctoGames.TestTask.Data;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units
{
    public sealed class UnitSceneService : MonoBehaviour, IDisposable
    {
        [Header("Setup")]
        [SerializeField] private OctoTestSettings settings;
        [SerializeField] private UnitView unitPrefab;
        [SerializeField] private Transform unitsRoot;
        [SerializeField] private Transform spawnZoneOrigin;

        [Header("Gizmos")]
        [SerializeField] private bool drawSpawnPointGizmos = true;
        [SerializeField] private bool drawMovementPathGizmos = true;
        [SerializeField, Min(0.01f)] private float gizmoPointRadius = 0.08f;
        [SerializeField] private Color gizmoPointColor = new (0.2f, 0.8f, 1f, 0.85f);
        [SerializeField] private Color gizmoPathColor = new (0.2f, 0.8f, 1f, 0.35f);
        [SerializeField] private Color gizmoOriginColor = new (1f, 0.8f, 0.2f, 1f);

        private readonly Dictionary<int, UnitView> _viewsById = new ();
        private readonly List<UnitView> _pooledViews = new ();
        private UnitService _unitService;
        private UnitCatalog _unitCatalog;
        private OctoTestSettings _settings;

        public void Initialize(UnitService service, UnitCatalog unitCatalog, OctoTestSettings projectSettings)
        {
            Dispose();
            ValidateRequiredReferences();

            _unitService = service ?? throw new ArgumentNullException(nameof(service));
            _unitCatalog = unitCatalog ?? throw new ArgumentNullException(nameof(unitCatalog));
            _settings = projectSettings ?? throw new ArgumentNullException(nameof(projectSettings));
            _unitService.SetGridOrigin(spawnZoneOrigin != null ? spawnZoneOrigin.position : transform.position);
            _unitService.UnitAdded += OnUnitAdded;
            _unitService.UnitChanged += OnUnitChanged;
            _unitService.UnitRemoved += OnUnitRemoved;
            _unitService.Reset += OnReset;
        }

        public void Dispose()
        {
            if (_unitService != null)
            {
                _unitService.UnitAdded -= OnUnitAdded;
                _unitService.UnitChanged -= OnUnitChanged;
                _unitService.UnitRemoved -= OnUnitRemoved;
                _unitService.Reset -= OnReset;
            }

            _unitService = null;
            _unitCatalog = null;
            _settings = null;
            ClearViews();
        }

        private void Update()
        {
            if (_unitService == null || _settings == null)
            {
                return;
            }

            IReadOnlyList<UnitRuntimeState> units = _unitService.Units;
            for (int i = 0; i < units.Count; i++)
            {
                UnitRuntimeState unit = units[i];
                if (unit == null || !unit.IsMoving || !_viewsById.TryGetValue(unit.RuntimeId, out UnitView view) || view == null)
                {
                    continue;
                }

                Vector3 target = _unitService.Grid.GetWorldPosition(unit.ReservedPointIndex);
                Vector3 nextPosition = Vector3.MoveTowards(
                    view.transform.position,
                    target,
                    _settings.UnitMoveSpeed * Time.deltaTime);
                view.SetPosition(nextPosition);
                _unitService.UpdatePosition(unit.RuntimeId, nextPosition);

                if ((nextPosition - target).sqrMagnitude <= 0.0001f)
                {
                    _unitService.CompleteMove(unit.RuntimeId);
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawSpawnPointGizmos)
            {
                return;
            }

            OctoTestSettings sourceSettings = _settings != null ? _settings : settings;
            if (sourceSettings == null)
            {
                return;
            }

            Color previousColor = Gizmos.color;
            Vector3 origin = GetGridOrigin();
            int columns = sourceSettings.SpawnZoneColumns;
            int pointCount = sourceSettings.SpawnPointCount;
            float spacing = sourceSettings.SpawnPointSpacing;
            float radius = Mathf.Max(0.01f, gizmoPointRadius);

            Gizmos.color = gizmoOriginColor;
            Gizmos.DrawSphere(origin, radius * 1.25f);

            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                Vector3 point = GetPointPosition(origin, columns, spacing, pointIndex);

                Gizmos.color = gizmoPointColor;
                Gizmos.DrawWireSphere(point, radius);

                if (drawMovementPathGizmos && pointCount > 1)
                {
                    Vector3 nextPoint = GetPointPosition(origin, columns, spacing, (pointIndex + 1) % pointCount);
                    Gizmos.color = gizmoPathColor;
                    Gizmos.DrawLine(point, nextPoint);
                }
            }

            Gizmos.color = previousColor;
        }

        private void OnUnitAdded(UnitRuntimeState unit)
        {
            if (unit == null)
            {
                return;
            }

            UnitView view = GetOrCreateView();
            view.Bind(unit, _unitCatalog.GetByIdOrDefault(unit.DataId));
            _viewsById[unit.RuntimeId] = view;
        }

        private void OnUnitChanged(UnitRuntimeState unit)
        {
            if (unit == null || !_viewsById.TryGetValue(unit.RuntimeId, out UnitView view) || view == null)
            {
                return;
            }

            view.Refresh(unit, _unitCatalog.GetByIdOrDefault(unit.DataId));
        }

        private void OnUnitRemoved(int runtimeId)
        {
            if (!_viewsById.TryGetValue(runtimeId, out UnitView view) || view == null)
            {
                return;
            }

            view.gameObject.SetActive(false);
            _viewsById.Remove(runtimeId);
        }

        private void OnReset()
        {
            ClearViews();
        }

        private UnitView GetOrCreateView()
        {
            for (int i = 0; i < _pooledViews.Count; i++)
            {
                UnitView view = _pooledViews[i];
                if (view != null && !view.gameObject.activeSelf)
                {
                    return view;
                }
            }

            Transform root = unitsRoot != null ? unitsRoot : transform;
            UnitView createdView = Instantiate(unitPrefab, root);
            _pooledViews.Add(createdView);
            return createdView;
        }

        private void ClearViews()
        {
            foreach (KeyValuePair<int, UnitView> pair in _viewsById)
            {
                if (pair.Value != null)
                {
                    pair.Value.gameObject.SetActive(false);
                }
            }

            _viewsById.Clear();
        }

        private void ValidateRequiredReferences()
        {
            if (unitPrefab == null)
            {
                throw new InvalidOperationException($"{name} requires assigned unit prefab.");
            }
            
            unitPrefab.ValidateRequiredReferences();
        }

        private Vector3 GetGridOrigin()
        {
            return spawnZoneOrigin != null ? spawnZoneOrigin.position : transform.position;
        }

        private static Vector3 GetPointPosition(Vector3 origin, int columns, float spacing, int pointIndex)
        {
            int x = pointIndex % columns;
            int y = pointIndex / columns;
            return origin + new Vector3(x * spacing, y * spacing, 0f);
        }
    }
}
