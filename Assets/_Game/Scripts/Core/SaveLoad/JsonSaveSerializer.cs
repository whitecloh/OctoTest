using System;
using UnityEngine;

namespace OctoGames.TestTask.Core.SaveLoad
{
    public sealed class JsonSaveSerializer : ISaveSerializer
    {
        public string Serialize<T>(T data)
        {
            return ReferenceEquals(data, null) ? throw new ArgumentNullException(nameof(data)) : JsonUtility.ToJson(data, true);
        }

        public T Deserialize<T>(string json)
        {
            return string.IsNullOrWhiteSpace(json) ? throw new ArgumentException("Json content is empty.", nameof(json)) : JsonUtility.FromJson<T>(json);
        }
    }
}
