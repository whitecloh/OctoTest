using UnityEngine;

namespace OctoGames.TestTask.UI.Dashboard
{
    public abstract class DashboardWindowView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;

        private bool _isClosing;

        public bool IsVisible => gameObject.activeSelf;

        public virtual void Show()
        {
            _isClosing = false;

            gameObject.SetActive(true);
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        public virtual void Hide()
        {
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
