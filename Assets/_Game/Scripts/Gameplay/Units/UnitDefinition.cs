using System;
using UnityEngine;

namespace OctoGames.TestTask.Gameplay.Units
{
    [Serializable]
    public sealed class UnitDefinition
    {
        private const string DefaultId = "unit";

        [SerializeField] private string id = DefaultId;
        [SerializeField] private Sprite sprite;
        [SerializeField] private Color color = Color.white;
        [SerializeField] private int startValue = 1;

        public string Id => string.IsNullOrWhiteSpace(id) ? DefaultId : id.Trim();
        public Sprite Sprite => sprite;
        public Color Color => color;
        public int StartValue => startValue;
    }
}
