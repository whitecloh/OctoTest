using System;
using System.Collections.Generic;
using OctoGames.TestTask.App.Bootstrap;
using OctoGames.TestTask.Data;
using OctoGames.TestTask.Gameplay.Units.Data;
using OctoGames.TestTask.Gameplay.Units.Runtime;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units.Presentation
{
    public sealed class UnitsScenePresenter : MonoBehaviour, IDisposable
    {
        [Header("Setup")]
        [SerializeField] private GameConfig settings;
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
        private readonly List<UnitSnapshot> _units = new ();
        private readonly List<int> _idsToRelease = new ();
        private GameServices _services;
        private UnitQuery _unitQuery;
        private UnitCatalog _unitCatalog;
        private GameConfig _settings;

        public void Initialize(GameServices services)
        {
            Dispose();
            ValidateRequiredReferences();

            _services = services ?? throw new ArgumentNullException(nameof(services));
            _unitQuery = _services.UnitQuery;
            _unitCatalog = _services.UnitCatalog;
            _settings = _services.Config;
            _services.UnitGrid.SetOrigin(spawnZoneOrigin != null ? spawnZoneOrigin.position : transform.position);
        }

        public void Dispose()
        {
            _services = null;
            _unitQuery = null;
            _unitCatalog = null;
            _settings = null;
            ClearViews();
        }

        public void Refresh()
        {
            if (_unitQuery == null || _unitCatalog == null)
            {
                return;
            }

            _unitQuery.Fill(_units);
            PrepareReleaseList();

            for (int i = 0; i < _units.Count; i++)
            {
                UnitSnapshot unit = _units[i];
                if (!_viewsById.TryGetValue(unit.RuntimeId, out UnitView view) || view == null)
                {
                    view = GetOrCreateView();
                    view.Bind(unit, _unitCatalog.GetByIdOrDefault(unit.DataId));
                    _viewsById[unit.RuntimeId] = view;
                }
                else
                {
                    view.Refresh(unit, _unitCatalog.GetByIdOrDefault(unit.DataId));
                }

                view.SetPosition(unit.Position);
                _idsToRelease.Remove(unit.RuntimeId);
            }

            ReleaseMissingViews();
        }

        private void OnDrawGizmos()
        {
            if (!drawSpawnPointGizmos)
            {
                return;
            }

            GameConfig sourceSettings = _settings != null ? _settings : settings;
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

        private void PrepareReleaseList()
        {
            _idsToRelease.Clear();
            foreach (int runtimeId in _viewsById.Keys)
            {
                _idsToRelease.Add(runtimeId);
            }
        }

        private void ReleaseMissingViews()
        {
            for (int i = 0; i < _idsToRelease.Count; i++)
            {
                int runtimeId = _idsToRelease[i];
                if (!_viewsById.TryGetValue(runtimeId, out UnitView view) || view == null)
                {
                    _viewsById.Remove(runtimeId);
                    continue;
                }

                view.gameObject.SetActive(false);
                _viewsById.Remove(runtimeId);
            }
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
