using System;
using System.Collections.Generic;
using OctoGames.TestTask.UI.Dashboard;
using UnityEngine;

namespace OctoGames.TestTask.UI
{
    public sealed class UiRoot : MonoBehaviour
    {
        [SerializeField] private List<DashboardWindowView> dashboardWindows = new ();

        public UnitsDashboardView UnitsDashboardView => GetUnitsDashboardView();

        private void OnValidate()
        {
            dashboardWindows.RemoveAll(view => view == null);
        }

        public void ValidateRequiredReferences()
        {
            if (dashboardWindows == null || dashboardWindows.Count == 0)
            {
                throw new InvalidOperationException($"{name} requires at least one dashboard window.");
            }

            for (int i = 0; i < dashboardWindows.Count; i++)
            {
                if (dashboardWindows[i] == null)
                {
                    throw new InvalidOperationException($"{name} has an empty dashboard window reference at index {i}.");
                }
            }

            GetUnitsDashboardView().ValidateRequiredReferences();
        }

        public void HideAllWindows()
        {
            if (dashboardWindows == null)
            {
                return;
            }

            for (int i = 0; i < dashboardWindows.Count; i++)
            {
                dashboardWindows[i]?.Hide();
            }
        }

        private UnitsDashboardView GetUnitsDashboardView()
        {
            if (dashboardWindows != null)
            {
                for (int i = 0; i < dashboardWindows.Count; i++)
                {
                    if (dashboardWindows[i] is UnitsDashboardView unitsDashboardView)
                    {
                        return unitsDashboardView;
                    }
                }
            }

            throw new InvalidOperationException($"{name} requires assigned {nameof(UnitsDashboardView)}.");
        }
    }
}
