using UnityEngine;

namespace OctoGames.TestTask.UI.Popups
{
    public abstract class PopupView : MonoBehaviour
    {
        [SerializeField] private string popupId;
        [SerializeField] private CanvasGroup canvasGroup;

        private bool _isClosing;

        public string PopupId => string.IsNullOrWhiteSpace(popupId) ? GetType().Name : popupId.Trim();
        public bool IsVisible => gameObject.activeSelf;

        public virtual void Show()
        {
            _isClosing = false;

            if (canvasGroup == null)
            {
                gameObject.SetActive(true);
                return;
            }

            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual void Hide()
        {
            if (canvasGroup == null)
            {
                gameObject.SetActive(false);
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        public virtual void Close()
        {
            if (_isClosing)
            {
                return;
            }

            _isClosing = true;
            Hide();
        }
    }
}
