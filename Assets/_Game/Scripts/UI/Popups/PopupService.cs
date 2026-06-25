using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OctoGames.TestTask.UI.Popups
{
    public sealed class PopupService
    {
        private readonly Dictionary<Type, PopupView> _viewsByType = new ();
        private readonly Dictionary<string, PopupView> _viewsById = new (StringComparer.Ordinal);

        public void Register(IEnumerable<PopupView> popupViews)
        {
            if (popupViews == null)
            {
                return;
            }

            foreach (PopupView popupView in popupViews)
            {
                Register(popupView);
            }
        }

        private void Register(PopupView popupView)
        {
            if (popupView == null)
            {
                return;
            }

            _viewsByType[popupView.GetType()] = popupView;
            _viewsById[popupView.PopupId] = popupView;
            popupView.Hide();
        }

        public void UnregisterAll()
        {
            _viewsByType.Clear();
            _viewsById.Clear();
        }
        
        public DashboardPopupView LoadAndShow(DashboardPopupView prefab, Transform parent, PopupRequest request)
        {
            return LoadAndShow<DashboardPopupView, PopupRequest>(prefab, parent, request);
        }
        
        public static TPopup LoadAndShow<TPopup, TRequest>(TPopup prefab, Transform parent, TRequest request)
            where TPopup : PopupView, IPopupView<TRequest>
        {
            if (prefab == null)
            {
                Debug.LogWarning($"{nameof(PopupService)} cannot load popup: prefab is not assigned.");
                return null;
            }

            if (parent == null)
            {
                Debug.LogWarning($"{nameof(PopupService)} cannot load popup: parent is not assigned.");
                return null;
            }

            TPopup popup = Object.Instantiate(prefab, parent, false);

            if (!popup.Setup(request))
            {
                Object.Destroy(popup.gameObject);
                Debug.LogWarning($"{nameof(PopupService)} rejected loaded {typeof(TPopup).Name} request.");
                return null;
            }

            popup.Show();
            return popup;
        }

        public DashboardPopupView Show(PopupRequest request)
        {
            return Show<DashboardPopupView, PopupRequest>(request);
        }

        public DashboardPopupView Show(string popupId, PopupRequest request)
        {
            return Show<DashboardPopupView, PopupRequest>(popupId, request);
        }

        public TPopup Show<TPopup, TRequest>(TRequest request)
            where TPopup : PopupView, IPopupView<TRequest>
        {
            if (!_viewsByType.TryGetValue(typeof(TPopup), out PopupView popupView) || popupView == null)
            {
                Debug.LogWarning($"{nameof(PopupService)} cannot show {typeof(TPopup).Name}: view is not registered.");
                return null;
            }

            return ShowResolved<TPopup, TRequest>(popupView, request);
        }

        public TPopup Show<TPopup, TRequest>(string popupId, TRequest request)
            where TPopup : PopupView, IPopupView<TRequest>
        {
            if (string.IsNullOrWhiteSpace(popupId) ||
                !_viewsById.TryGetValue(popupId.Trim(), out PopupView popupView) ||
                popupView == null)
            {
                Debug.LogWarning($"{nameof(PopupService)} cannot show popup '{popupId}': view is not registered.");
                return null;
            }

            return ShowResolved<TPopup, TRequest>(popupView, request);
        }

        private static TPopup ShowResolved<TPopup, TRequest>(PopupView popupView, TRequest request)
            where TPopup : PopupView, IPopupView<TRequest>
        {
            TPopup typedPopup = popupView as TPopup;
            if (typedPopup == null || !typedPopup.Setup(request))
            {
                Debug.LogWarning($"{nameof(PopupService)} rejected {typeof(TPopup).Name} request.");
                return null;
            }

            typedPopup.Show();
            return typedPopup;
        }
    }
}
