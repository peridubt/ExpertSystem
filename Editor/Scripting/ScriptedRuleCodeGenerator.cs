using System;
using System.IO;
using System.Text;
using ExpertSystem.Rules.Scripted;
using UnityEditor;

namespace ExpertSystem.Editor.Scripting
{
    public static class ScriptedRuleCodeGenerator
    {
        public const string GeneratedDir = "Assets/ExpertSystem/Rules/Scripted/Generated";
        public const string GeneratedNamespace = "ExpertSystem.Rules.Scripted.Generated";

        [MenuItem("Tools/Expert System/Build All Scripted Rules")]
        public static void BuildAllMenuItem()
        {
            BuildAll();
        }

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
            UnityEngine.Debug.Log($"[ExpertSystem] Built {count} scripted rule(s).");
        }

        public static void Build(ScriptedRuleAsset asset)
        {
            if (asset == null) return;
            if (string.IsNullOrWhiteSpace(asset.defineBody))
            {
                UnityEngine.Debug.LogWarning($"[ExpertSystem] Skipping '{asset.ruleName}': empty body.");
                return;
            }

            EnsureGeneratedDir();

            var className = MakeClassName(asset.ruleName);
            var code = Generate(asset, className);
            var path = Path.Combine(GeneratedDir, className + ".cs");
            File.WriteAllText(path, code);
            AssetDatabase.ImportAsset(path);
        }

        private static string Generate(ScriptedRuleAsset asset, string className)
        {
            var sb = new StringBuilder();
            sb.AppendLine("// Auto-generated from ScriptedRuleAsset. Do not edit by hand.");
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

        private static string Escape(string s)
        {
            if (s == null) return string.Empty;
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
        }

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
