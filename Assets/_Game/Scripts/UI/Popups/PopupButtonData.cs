using System;

namespace OctoGames.TestTask.UI.Popups
{
    public sealed class PopupButtonData
    {
        public PopupButtonData(string text, Action callback = null, bool closeAfterClick = true)
        {
            Text = string.IsNullOrWhiteSpace(text) ? "OK" : text;
            Callback = callback;
            CloseAfterClick = closeAfterClick;
        }

        public string Text { get; }
        public Action Callback { get; }
        public bool CloseAfterClick { get; }
    }
}
