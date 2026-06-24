using System;
using System.Collections.Generic;
using UnityEngine;

namespace OctoGames.TestTask.UI.Popups
{
    public sealed class PopupButtonPanelView : MonoBehaviour
    {
        private const int MinButtons = 1;
        private const int MaxButtons = 5;

        [SerializeField] private Transform buttonsRoot;
        [SerializeField] private PopupButtonView buttonPrefab;

        private readonly List<PopupButtonView> _buttons = new (MaxButtons);

        public static bool HasValidButtonCount(IReadOnlyList<PopupButtonData> buttons)
        {
            int count = buttons?.Count ?? 0;
            return count is >= MinButtons and <= MaxButtons;
        }

        public bool Setup(IReadOnlyList<PopupButtonData> buttons, Action<PopupButtonData> clicked)
        {
            ValidateRequiredReferences();
            if (!HasValidButtonCount(buttons))
            {
                return false;
            }

            EnsureButtonCount(buttons.Count);

            for (int i = 0; i < _buttons.Count; i++)
            {
                PopupButtonView buttonView = _buttons[i];
                if (buttonView == null)
                {
                    continue;
                }

                if (i >= buttons.Count)
                {
                    Release(buttonView);
                    continue;
                }

                buttonView.Setup(buttons[i] ?? new PopupButtonData("OK"), clicked);
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
                PopupButtonView button = _buttons[i];
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
                PopupButtonView buttonView = Instantiate(buttonPrefab, buttonsRoot, false);
                buttonView.gameObject.SetActive(false);
                _buttons.Add(buttonView);
            }
        }

        private static void Release(PopupButtonView button)
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
