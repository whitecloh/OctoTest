using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace OctoGames.TestTask.UI.Dashboard
{
    public sealed class UnitsDashboardView : DashboardWindowView
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private DashboardButtonPanelView buttonPanel;
        [SerializeField] private UnitsStatsView statsView;

        public UnitsStatsView StatsView => statsView;

        public void Show(string title, string body, IReadOnlyList<DashboardButtonData> buttons)
        {
            ValidateRequiredReferences();

            if (buttons == null || buttons.Count == 0)
            {
                throw new InvalidOperationException($"{name} requires at least one dashboard button.");
            }

            titleText.text = title;
            bodyText.text = body;

            buttonPanel.Setup(buttons, HandleButtonClicked);
            base.Show();
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

            if (statsView == null)
            {
                throw new InvalidOperationException($"{name} requires assigned stats view reference.");
            }

            buttonPanel.ValidateRequiredReferences();
            statsView.ValidateRequiredReferences();
        }

        private void HandleButtonClicked(DashboardButtonData buttonData)
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
