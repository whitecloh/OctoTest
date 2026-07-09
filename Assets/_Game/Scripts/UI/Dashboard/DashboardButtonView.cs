using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace OctoGames.TestTask.UI.Dashboard
{
    public sealed class DashboardButtonView : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI label;

        private DashboardButtonData _data;
        private Action<DashboardButtonData> _clicked;
        private bool _isBound;

        public void Setup(DashboardButtonData buttonData, Action<DashboardButtonData> clickCallback)
        {
            ValidateRequiredReferences();

            _data = buttonData ?? new DashboardButtonData("OK");
            _clicked = clickCallback;
            button.onClick.RemoveListener(OnClicked);
            _isBound = false;

            label.text = _data.Text;

            Bind();
            gameObject.SetActive(true);
        }

        public void Clear()
        {
            Unbind();
            _data = null;
            _clicked = null;
        }

        public void ValidateRequiredReferences()
        {
            if (button == null || label == null)
            {
                throw new InvalidOperationException($"{name} requires assigned button and label references.");
            }
        }

        private void OnEnable()
        {
            Bind();
        }

        private void OnDisable()
        {
            Unbind();
        }

        private void OnDestroy()
        {
            Clear();
        }

        private void Bind()
        {
            if (_isBound || button == null || _clicked == null)
            {
                return;
            }

            button.onClick.RemoveListener(OnClicked);
            button.onClick.AddListener(OnClicked);
            _isBound = true;
        }

        private void Unbind()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClicked);
            }

            _isBound = false;
        }

        private void OnClicked()
        {
            _clicked?.Invoke(_data);
        }
    }
}
