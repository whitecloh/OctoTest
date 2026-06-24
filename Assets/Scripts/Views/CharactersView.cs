using System;
using OctoGames.TestTask.Gameplay.Units;
using TMPro;
using UnityEngine;

namespace OctoGames.TestTask.Views
{
    public sealed class CharactersView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI label;

        public void SetStats(UnitStats stats, string format)
        {
            ValidateRequiredReferences();
            label.text = FormatStats(stats, format);
        }

        public void ValidateRequiredReferences()
        {
            if (label == null)
            {
                throw new InvalidOperationException($"{name} requires assigned label reference.");
            }
        }

        private static string FormatStats(UnitStats stats, string format)
        {
            string safeFormat = string.IsNullOrWhiteSpace(format)
                ? "Units: {0}/{1} | Value: {2} | Avg: {3:0.##}"
                : format;

            try
            {
                return string.Format(
                    safeFormat,
                    stats.TotalCount,
                    stats.MaxCount,
                    stats.TotalValue,
                    stats.AverageValue);
            }
            catch (FormatException)
            {
                return string.Format(
                    "Units: {0}/{1} | Value: {2} | Avg: {3:0.##}",
                    stats.TotalCount,
                    stats.MaxCount,
                    stats.TotalValue,
                    stats.AverageValue);
        }
    }
}
}
