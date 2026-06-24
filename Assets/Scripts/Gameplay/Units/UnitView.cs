using System;
using TMPro;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units
{
    public sealed class UnitView : MonoBehaviour
    {
        [SerializeField] private TMP_Text valueText;

        public void Bind(UnitRuntimeState state)
        {
            ValidateRequiredReferences();
            
            if (state == null)
            {
                return;
            }

            transform.position = state.Position;
            gameObject.SetActive(true);
            Refresh(state);
        }

        public void Refresh(UnitRuntimeState state)
        {
            if (state == null)
            {
                return;
            }

            valueText.text = state.Value.ToString();
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void ValidateRequiredReferences()
        {
            if (valueText == null)
            {
                throw new InvalidOperationException($"{name} requires assigned value text reference.");
            }
        }
    }
}
