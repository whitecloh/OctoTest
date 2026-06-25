using System;
using System.Collections.Generic;

namespace OctoGames.TestTask.UI.Popups
{
    public sealed class PopupRequest
    {
        public PopupRequest(string title, string body, IReadOnlyList<PopupButtonData> buttons)
        {
            Title = title ?? string.Empty;
            Body = body ?? string.Empty;
            Buttons = CopyButtons(buttons);
        }

        public string Title { get; }
        public string Body { get; }
        public IReadOnlyList<PopupButtonData> Buttons { get; }

        private static PopupButtonData[] CopyButtons(IReadOnlyList<PopupButtonData> buttons)
        {
            if (buttons == null || buttons.Count == 0)
            {
                return Array.Empty<PopupButtonData>();
            }

            PopupButtonData[] copy = new PopupButtonData[buttons.Count];
            for (int i = 0; i < buttons.Count; i++)
            {
                copy[i] = buttons[i];
            }

            return copy;
        }
    }
}
