using UnityEngine;

namespace OctoGames.TestTask.Data
{
    [CreateAssetMenu(menuName = "Octo Games/Test Task/Settings")]
    public sealed class OctoTestSettings : ScriptableObject
    {
        private const string DefaultSaveDirectoryName = "Saves";
        private const string DefaultUnitsSaveFileName = "units_state";
        private const string DefaultUnitsLabelFormat = "Units: {0}/{1} | Value: {2} | Avg: {3:0.##}";

        [Header("Save")]
        [SerializeField] private string saveDirectoryName = DefaultSaveDirectoryName;
        [SerializeField] private string unitsSaveFileName = DefaultUnitsSaveFileName;

        [Header("Spawn Zone")]
        [SerializeField, Min(1)] private int spawnZoneColumns = 8;
        [SerializeField, Min(1)] private int spawnZoneRows = 10;
        [SerializeField, Min(0.1f)] private float spawnPointSpacing = 1f;

        [Header("Units")]
        [SerializeField, Min(0)] private int initialUnits = 5;
        [SerializeField, Min(1)] private int maxUnits = 20;
        [SerializeField, Min(0.1f)] private float unitMoveSpeed = 4f;
        [SerializeField] private int valueIncreaseAmount = 1;

        [Header("Units View")]
        [SerializeField, Min(0.05f)] private float unitsViewUpdateInterval = 0.25f;
        [SerializeField] private string unitsLabelFormat = DefaultUnitsLabelFormat;

        public string SaveDirectoryName =>
            string.IsNullOrWhiteSpace(saveDirectoryName) ? DefaultSaveDirectoryName : saveDirectoryName.Trim();

        public string UnitsSaveFileName =>
            string.IsNullOrWhiteSpace(unitsSaveFileName) ? DefaultUnitsSaveFileName : unitsSaveFileName.Trim();

        public int SpawnZoneColumns => Mathf.Max(1, spawnZoneColumns);
        public int SpawnZoneRows => Mathf.Max(1, spawnZoneRows);
        public int SpawnPointCount => SpawnZoneColumns * SpawnZoneRows;
        public float SpawnPointSpacing => Mathf.Max(0.1f, spawnPointSpacing);

        public int InitialUnits => Mathf.Clamp(initialUnits, 0, MaxUnits);
        public int MaxUnits => Mathf.Clamp(maxUnits, 1, SpawnPointCount);
        public float UnitMoveSpeed => Mathf.Max(0.1f, unitMoveSpeed);
        public int ValueIncreaseAmount => valueIncreaseAmount;

        public float UnitsViewUpdateInterval => Mathf.Max(0.05f, unitsViewUpdateInterval);

        public string UnitsLabelFormat =>
            string.IsNullOrWhiteSpace(unitsLabelFormat)
                ? DefaultUnitsLabelFormat
                : unitsLabelFormat;
    }
}
