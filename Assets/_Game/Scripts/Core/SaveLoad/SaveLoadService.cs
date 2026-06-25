using System;
using System.IO;
using UnityEngine;

namespace OctoGames.TestTask.Core.SaveLoad
{
    public sealed class SaveLoadService : ISaveLoadService
    {
        private const string DefaultExtension = ".json";

        private readonly ISaveSerializer _serializer;
        private readonly string _saveDirectory;

        public SaveLoadService(ISaveSerializer serializer, string saveDirectory = null)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _saveDirectory = string.IsNullOrWhiteSpace(saveDirectory)
                ? Path.Combine(Application.persistentDataPath, "Saves")
                : saveDirectory;
        }

        public bool Save<T>(string fileName, T data)
        {
            if (ReferenceEquals(data, null) || !TryBuildPath(fileName, out string path, out _))
            {
                return false;
            }

            string tempPath = path + ".tmp";
            try
            {
                Directory.CreateDirectory(_saveDirectory);
                string json = _serializer.Serialize(data);
                string backupPath = path + ".bak";
                File.WriteAllText(tempPath, json);

                if (File.Exists(path))
                {
                    ReplaceExistingSave(tempPath, path, backupPath);
                }
                else
                {
                    File.Move(tempPath, path);
                }

                File.Delete(tempPath);
                return true;
            }
            catch (Exception)
            {
                TryDeleteTempFile(tempPath);
                return false;
            }
        }

        private static void TryDeleteTempFile(string tempPath)
        {
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch (Exception)
            {
                // Ignored: failed save cleanup should not hide the original save failure.
            }
        }

        private static void ReplaceExistingSave(string tempPath, string path, string backupPath)
        {
            try
            {
                File.Replace(tempPath, path, backupPath, true);
            }
            catch (PlatformNotSupportedException)
            {
                File.Copy(path, backupPath, true);
                File.Copy(tempPath, path, true);
            }
        }

        public SaveLoadResult<T> Load<T>(string fileName, T defaultValue = default)
        {
            if (!TryBuildPath(fileName, out string path, out string pathError))
            {
                return SaveLoadResult<T>.Failed(defaultValue, pathError);
            }

            if (!File.Exists(path))
            {
                return SaveLoadResult<T>.Failed(defaultValue, $"Save file not found: {path}");
            }

            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return SaveLoadResult<T>.Failed(defaultValue, "Save file is empty.");
                }

                T data = _serializer.Deserialize<T>(json);
                if (ReferenceEquals(data, null))
                {
                    return SaveLoadResult<T>.Failed(defaultValue, "Save data could not be deserialized.");
                }

                return SaveLoadResult<T>.Succeeded(data);
            }
            catch (Exception exception)
            {
                return SaveLoadResult<T>.Failed(defaultValue, exception.Message);
            }
        }

        public bool Exists(string fileName)
        {
            return TryBuildPath(fileName, out string path, out _) && File.Exists(path);
        }

        public bool Delete(string fileName)
        {
            if (!TryBuildPath(fileName, out string path, out _) || !File.Exists(path))
            {
                return false;
            }

            try
            {
                File.Delete(path);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool TryBuildPath(string fileName, out string path, out string error)
        {
            path = string.Empty;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                error = "Save file name is empty.";
                return false;
            }

            string normalizedFileName = fileName.Trim();
            if (ContainsInvalidFileNameCharacter(normalizedFileName))
            {
                error = "Save file name contains invalid characters.";
                return false;
            }

            try
            {
                if (Path.IsPathRooted(normalizedFileName) ||
                    normalizedFileName.IndexOf(Path.DirectorySeparatorChar) >= 0 ||
                    normalizedFileName.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
                {
                    error = "Save file name must not contain a directory path.";
                    return false;
                }

                if (string.IsNullOrEmpty(Path.GetExtension(normalizedFileName)))
                {
                    normalizedFileName += DefaultExtension;
                }

                path = Path.Combine(_saveDirectory, normalizedFileName);
                return true;
            }
            catch (Exception exception)
            {
                error = exception.Message;
                return false;
            }
        }

        private static bool ContainsInvalidFileNameCharacter(string fileName)
        {
            char[] invalidCharacters = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalidCharacters.Length; i++)
            {
                if (fileName.IndexOf(invalidCharacters[i]) >= 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
