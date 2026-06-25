using System;
using TMPro;
using UnityEngine;
using OctoGames.TestTask.Views;

namespace OctoGames.TestTask.UI.Popups
{
    public sealed class DashboardPopupView : PopupView, IPopupView<PopupRequest>
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private PopupButtonPanelView buttonPanel;
        [SerializeField] private CharactersView charactersView;

        public CharactersView CharactersView => charactersView;

        public bool Setup(PopupRequest request)
        {
            ValidateRequiredReferences();
            if (request == null || !PopupButtonPanelView.HasValidButtonCount(request.Buttons))
            {
                return false;
            }

            titleText.text = request.Title;
            bodyText.text = request.Body;

            return buttonPanel.Setup(request.Buttons, HandleButtonClicked);
        }

        public override void Close()
        {
            buttonPanel?.Clear();
            base.Close();
        }

        public void ValidateRequiredReferences()
        {
            if (titleText == null || bodyText == null || buttonPanel == null)
            {
                throw new InvalidOperationException($"{name} requires assigned title, body and button panel references.");
            }

            if (charactersView == null)
            {
                throw new InvalidOperationException($"{name} requires assigned characters view reference.");
            }

            buttonPanel.ValidateRequiredReferences();
            charactersView.ValidateRequiredReferences();
        }

        private void HandleButtonClicked(PopupButtonData buttonData)
        {
            try
            {
                buttonData?.Callback?.Invoke();
            }
            finally
            {
                if (buttonData == null || buttonData.CloseAfterClick)
                {
                    Close();
                }
            }
        }
    }
}
