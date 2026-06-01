// Низкоуровневое чтение и запись объектов в JSON-файлы средствами Unity (JsonUtility).
using System.IO;
using System.Text;
using UnityEngine;

namespace ExpertSystem.RuleEngine.Core.Data
{
    /// <summary>Сериализация/десериализация DTO в файлы в кодировке UTF-8.</summary>
    public static class JsonFileStore
    {
        /// <summary>
        /// Читает файл и десериализует его в объект типа T. Принимает путь к файлу.
        /// Возвращает объект или null, если файла нет.
        /// </summary>
        public static T ReadFromFile<T>(string path) where T : class
        {
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[JsonFileStore] Файл не найден: {path}");
                return null;
            }
            var json = File.ReadAllText(path, Encoding.UTF8);
            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// Сериализует объект и записывает в файл, создавая директорию при необходимости.
        /// Принимает путь, объект и флаг форматирования.
        /// </summary>
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
