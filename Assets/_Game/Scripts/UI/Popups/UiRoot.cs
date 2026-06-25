using System;
using System.Collections.Generic;
using UnityEngine;

namespace OctoGames.TestTask.UI.Popups
{
    public sealed class UiRoot : MonoBehaviour
    {
        [SerializeField] private List<PopupView> popupViews = new ();

        public IReadOnlyList<PopupView> PopupViews => popupViews;

        private void OnValidate()
        {
            popupViews.RemoveAll(view => view == null);
        }

        public void ValidateRequiredReferences()
        {
            if (popupViews == null || popupViews.Count == 0)
            {
                throw new InvalidOperationException($"{name} requires at least one popup view.");
            }

            for (int i = 0; i < popupViews.Count; i++)
            {
                if (popupViews[i] == null)
                {
                    throw new InvalidOperationException($"{name} has an empty popup view reference at index {i}.");
                }

                if (popupViews[i] is DashboardPopupView textPopupView)
                {
                    textPopupView.ValidateRequiredReferences();
                }
            }
        }
    }
}
