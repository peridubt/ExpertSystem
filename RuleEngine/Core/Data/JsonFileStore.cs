using System.IO;
using System.Text;
using UnityEngine;

namespace ExpertSystem.RuleEngine.Core.Data
{
    public static class JsonFileStore
    {
        public static T ReadFromFile<T>(string path) where T : class
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[JsonFileStore] File not found: {path}");
                return null;
            }
            var json = File.ReadAllText(path, Encoding.UTF8);
            return JsonUtility.FromJson<T>(json);
        }

        public static void WriteToFile<T>(string path, T value, bool prettyPrint = true) where T : class
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var json = JsonUtility.ToJson(value, prettyPrint);
            File.WriteAllText(path, json, Encoding.UTF8);
        }
    }
}
