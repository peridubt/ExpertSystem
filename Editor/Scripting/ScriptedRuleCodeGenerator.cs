// Кодогенератор скриптовых правил. Оборачивает введённое пользователем тело метода
// Define() в шаблон класса правила NRules и сохраняет .cs-файл в сборку правил.
using System;
using System.IO;
using System.Text;
using ExpertSystem.Rules.Scripted;
using UnityEditor;

namespace ExpertSystem.Editor.Scripting
{
    /// <summary>Статический генератор кода из ресурсов ScriptedRuleAsset.</summary>
    public static class ScriptedRuleCodeGenerator
    {
        public const string GeneratedDir = "Assets/ExpertSystem/Rules/Scripted/Generated";
        public const string GeneratedNamespace = "ExpertSystem.Rules.Scripted.Generated";

        /// <summary>Пункт меню: собрать все скриптовые правила в проекте.</summary>
        [MenuItem("Tools/Expert System/Build All Scripted Rules")]
        public static void BuildAllMenuItem()
        {
            BuildAll();
        }

        /// <summary>Находит все ресурсы скриптовых правил и генерирует для каждого код.</summary>
        public static void BuildAll()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(ScriptedRuleAsset));
            int count = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptedRuleAsset>(path);
                if (asset == null) continue;
                Build(asset);
                count++;
            }
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"[ExpertSystem] Собрано скриптовых правил: {count}.");
        }

        /// <summary>Генерирует .cs-файл для одного ресурса. Принимает ресурс; пропускает пустое тело.</summary>
        public static void Build(ScriptedRuleAsset asset)
        {
            if (asset == null) return;
            if (string.IsNullOrWhiteSpace(asset.defineBody))
            {
                UnityEngine.Debug.LogWarning($"[ExpertSystem] Пропуск '{asset.ruleName}': пустое тело.");
                return;
            }

            EnsureGeneratedDir();

            var className = MakeClassName(asset.ruleName);
            var code = Generate(asset, className);
            var path = Path.Combine(GeneratedDir, className + ".cs");
            File.WriteAllText(path, code);
            AssetDatabase.ImportAsset(path);
        }

        /// <summary>
        /// Оборачивает тело Define() в класс правила. Принимает ресурс и имя класса,
        /// возвращает исходный код. Тело пользователя вставляется построчно с отступом.
        /// </summary>
        private static string Generate(ScriptedRuleAsset asset, string className)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// Автоматически сгенерировано из ScriptedRuleAsset. Не редактировать вручную.");
            sb.AppendLine("using ExpertSystem.RuleEngine.Core.Domain;");
            sb.AppendLine("using ExpertSystem.RuleEngine.Core.Rules;");
            sb.AppendLine("using NRules.Fluent.Dsl;");
            sb.AppendLine();
            sb.AppendLine($"namespace {GeneratedNamespace}");
            sb.AppendLine("{");
            sb.AppendLine($"    [Name(\"{Escape(asset.ruleName)}\")]");
            if (asset.priority != 0)
            {
                sb.AppendLine($"    [Priority({asset.priority})]");
            }
            sb.AppendLine($"    public class {className} : Rule");
            sb.AppendLine("    {");
            sb.AppendLine("        public override void Define()");
            sb.AppendLine("        {");
            foreach (var line in asset.defineBody.Replace("\r\n", "\n").Split('\n'))
            {
                sb.AppendLine(line.Length == 0 ? string.Empty : "            " + line);
            }
            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// Превращает имя правила в корректное имя класса C#. Принимает строку, возвращает
        /// PascalCase-идентификатор с суффиксом Rule.
        /// </summary>
        private static string MakeClassName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return "UnnamedRule";

            var sb = new StringBuilder();
            bool capitalizeNext = true;
            foreach (var c in raw)
            {
                if (char.IsLetterOrDigit(c))
                {
                    sb.Append(capitalizeNext ? char.ToUpperInvariant(c) : c);
                    capitalizeNext = false;
                }
                else
                {
                    capitalizeNext = true;
                }
            }

            if (sb.Length == 0) return "UnnamedRule";
            if (char.IsDigit(sb[0])) sb.Insert(0, "_");

            var name = sb.ToString();
            if (!name.EndsWith("Rule", StringComparison.Ordinal)) name += "Rule";
            return name;
        }

        /// <summary>Экранирует спецсимволы для вставки строки в исходный код.</summary>
        private static string Escape(string s)
        {
            if (s == null) return string.Empty;
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

        /// <summary>Создаёт папку для сгенерированных файлов, если её ещё нет.</summary>
        private static void EnsureGeneratedDir()
        {
            if (!Directory.Exists(GeneratedDir))
            {
                Directory.CreateDirectory(GeneratedDir);
                AssetDatabase.Refresh();
            }
        }
    }
}
