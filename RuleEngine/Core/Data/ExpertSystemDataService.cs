// Сервис ввода/вывода: перечисление, чтение и запись наборов фактов и результатов
// вывода в формате JSON в папке ExpertSystem/Data/Facts.
using System;
using System.Collections.Generic;
using System.IO;

namespace ExpertSystem.RuleEngine.Core.Data
{
    /// <summary>Высокоуровневый доступ к хранилищу фактов и результатов.</summary>
    public class ExpertSystemDataService
    {
        public const string FactsFolderName = "Facts";
        public const string ResultsFolderName = "Results";

        /// <summary>Папка с наборами фактов.</summary>
        public string FactsRoot => Path.Combine(DataPaths.AuthoredDataRoot, FactsFolderName);

        /// <summary>Папка с сохранёнными результатами вывода.</summary>
        public string ResultsRoot => Path.Combine(DataPaths.AuthoredDataRoot, FactsFolderName, ResultsFolderName);

        /// <summary>
        /// Возвращает идентификаторы всех наборов фактов (имена JSON-файлов без расширения),
        /// отсортированные по алфавиту. Пустой список, если папки нет.
        /// </summary>
        public IReadOnlyList<string> ListFactSetIds()
        {
            var dir = FactsRoot;
            if (!Directory.Exists(dir)) return Array.Empty<string>();

            var ids = new List<string>();
            foreach (var path in Directory.GetFiles(dir, "*.json", SearchOption.TopDirectoryOnly))
            {
                ids.Add(Path.GetFileNameWithoutExtension(path));
            }
            ids.Sort(StringComparer.OrdinalIgnoreCase);
            return ids;
        }

        /// <summary>Загружает набор фактов по идентификатору. Возвращает DTO или null.</summary>
        public FactSetData LoadFactSet(string factSetId)
        {
            if (string.IsNullOrEmpty(factSetId)) return null;
            var path = Path.Combine(FactsRoot, factSetId + ".json");
            return JsonFileStore.ReadFromFile<FactSetData>(path);
        }

        /// <summary>Сохраняет набор фактов (имя файла = factSetId). Обновляет метку времени.</summary>
        public void SaveFactSet(FactSetData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(data.factSetId))
                throw new ArgumentException("Не задан factSetId.", nameof(data));

            data.updatedAtUtc = DateTime.UtcNow.ToString("O");
            var path = Path.Combine(FactsRoot, data.factSetId + ".json");
            JsonFileStore.WriteToFile(path, data);
        }

        /// <summary>
        /// Сохраняет результат вывода в папку Results. Принимает DTO результата и имя файла;
        /// при пустом имени генерирует его из текущей даты-времени.
        /// </summary>
        public void SaveDecisionResult(DecisionResultData result, string fileNameWithoutExtension)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (string.IsNullOrEmpty(fileNameWithoutExtension))
                fileNameWithoutExtension = "result-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

            var path = Path.Combine(ResultsRoot, fileNameWithoutExtension + ".json");
            JsonFileStore.WriteToFile(path, result);
        }

        /// <summary>Загружает ранее сохранённый результат по имени файла. Возвращает DTO или null.</summary>
        public DecisionResultData LoadDecisionResult(string fileNameWithoutExtension)
        {
            if (string.IsNullOrEmpty(fileNameWithoutExtension)) return null;
            var path = Path.Combine(ResultsRoot, fileNameWithoutExtension + ".json");
            return JsonFileStore.ReadFromFile<DecisionResultData>(path);
        }
    }
}
