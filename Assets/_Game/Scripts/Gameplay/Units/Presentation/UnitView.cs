using System;
using OctoGames.TestTask.Gameplay.Units.Data;
using OctoGames.TestTask.Gameplay.Units.Runtime;
using TMPro;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units.Presentation
{
    public sealed class UnitView : MonoBehaviour
    {
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Bind(UnitSnapshot state, UnitDefinition definition)
        {
            ValidateRequiredReferences();

            transform.position = state.Position;
            gameObject.SetActive(true);
            Refresh(state, definition);
        }

        public void Refresh(UnitSnapshot state, UnitDefinition definition)
        {
            if (definition != null)
            {
                spriteRenderer.color = definition.Color;
                
                if (definition.Sprite != null)
                {
                    spriteRenderer.sprite = definition.Sprite;
                }
            }

            valueText.text = state.Value.ToString();
        }

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void ValidateRequiredReferences()
        {
            if (valueText == null || spriteRenderer == null)
            {
                throw new InvalidOperationException($"{name} requires assigned value text and sprite renderer references.");
            }
        }
    }
}
