// Вычисление путей к данным проекта (папка с наборами фактов в формате JSON).
using System.IO;

namespace ExpertSystem.RuleEngine.Core.Data
{
    /// <summary>Единая точка получения корневого пути к авторским данным.</summary>
    public static class DataPaths
    {
        /// <summary>Папка ExpertSystem/Data относительно каталога ресурсов проекта.</summary>
        public static string AuthoredDataRoot => Path.Combine(UnityEngine.Application.dataPath, "ExpertSystem", "Data");
    }
}
