using System;
using System.Collections.Generic;
using System.IO;

namespace ExpertSystem.RuleEngine.Core.Data
{
    public class ExpertSystemDataService
    {
        public const string FactsFolderName = "Facts";
        public const string ResultsFolderName = "Results";

        public string FactsRoot => Path.Combine(DataPaths.AuthoredDataRoot, FactsFolderName);
        public string ResultsRoot => Path.Combine(DataPaths.AuthoredDataRoot, FactsFolderName, ResultsFolderName);

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

        public FactSetData LoadFactSet(string factSetId)
        {
            if (string.IsNullOrEmpty(factSetId)) return null;
            var path = Path.Combine(FactsRoot, factSetId + ".json");
            return JsonFileStore.ReadFromFile<FactSetData>(path);
        }

        public void SaveFactSet(FactSetData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (string.IsNullOrEmpty(data.factSetId))
                throw new ArgumentException("factSetId is required.", nameof(data));

            data.updatedAtUtc = DateTime.UtcNow.ToString("O");
            var path = Path.Combine(FactsRoot, data.factSetId + ".json");
            JsonFileStore.WriteToFile(path, data);
        }

        public void SaveDecisionResult(DecisionResultData result, string fileNameWithoutExtension)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));
            if (string.IsNullOrEmpty(fileNameWithoutExtension))
                fileNameWithoutExtension = "result-" + DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");

            var path = Path.Combine(ResultsRoot, fileNameWithoutExtension + ".json");
            JsonFileStore.WriteToFile(path, result);
        }

        public DecisionResultData LoadDecisionResult(string fileNameWithoutExtension)
        {
            if (string.IsNullOrEmpty(fileNameWithoutExtension)) return null;
            var path = Path.Combine(ResultsRoot, fileNameWithoutExtension + ".json");
            return JsonFileStore.ReadFromFile<DecisionResultData>(path);
        }
    }
}
