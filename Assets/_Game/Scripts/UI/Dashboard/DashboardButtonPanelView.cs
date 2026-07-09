using System;
using System.Collections.Generic;
using UnityEngine;

namespace OctoGames.TestTask.UI.Dashboard
{
    public sealed class DashboardButtonPanelView : MonoBehaviour
    {
        [SerializeField] private Transform buttonsRoot;
        [SerializeField] private DashboardButtonView buttonPrefab;

        private readonly List<DashboardButtonView> _buttons = new ();

        public bool Setup(IReadOnlyList<DashboardButtonData> buttons, Action<DashboardButtonData> clicked)
        {
            ValidateRequiredReferences();
            
            EnsureButtonCount(buttons.Count);

            for (int i = 0; i < _buttons.Count; i++)
            {
                DashboardButtonView buttonView = _buttons[i];
                if (buttonView == null)
                {
                    continue;
                }

                if (i >= buttons.Count)
                {
                    Release(buttonView);
                    continue;
                }

                buttonView.Setup(buttons[i] ?? new DashboardButtonData("OK"), clicked);
            }

            return true;
        }

        public void Clear()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                Release(_buttons[i]);
            }
        }

        public void ValidateRequiredReferences()
        {
            if (buttonsRoot == null || buttonPrefab == null)
            {
                throw new InvalidOperationException($"{name} requires assigned button root and prefab.");
            }

            buttonPrefab.ValidateRequiredReferences();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                DashboardButtonView button = _buttons[i];
                if (button == null)
                {
                    continue;
                }

                button.Clear();
                Destroy(button.gameObject);
            }

            _buttons.Clear();
        }

        private void EnsureButtonCount(int count)
        {
            for (int i = _buttons.Count; i < count; i++)
            {
                DashboardButtonView buttonView = Instantiate(buttonPrefab, buttonsRoot, false);
                buttonView.gameObject.SetActive(false);
                _buttons.Add(buttonView);
            }
        }

        private static void Release(DashboardButtonView button)
        {
            if (button == null)
            {
                return;
            }

            button.Clear();
            button.gameObject.SetActive(false);
        }
    }
}
