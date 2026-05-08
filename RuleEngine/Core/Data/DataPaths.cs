using System.IO;

namespace ExpertSystem.RuleEngine.Core.Data
{
    public static class DataPaths
    {
        public static string AuthoredDataRoot => Path.Combine(UnityEngine.Application.dataPath, "ExpertSystem", "Data");
    }
}
