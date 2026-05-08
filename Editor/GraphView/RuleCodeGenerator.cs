using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ExpertSystem.Rules.Graph;
using UnityEditor;

namespace ExpertSystem.Editor.GraphView
{
    public static class RuleCodeGenerator
    {
        public const string GeneratedDir = "Assets/ExpertSystem/Rules/Graph/Generated";
        public const string GeneratedNamespace = "ExpertSystem.Rules.Graph.Generated";

        [MenuItem("Tools/Expert System/Build All Rule Graphs")]
        public static void BuildAllMenuItem()
        {
            BuildAll();
        }

        public static void BuildAll()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(RuleGraphAsset));
            int count = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<RuleGraphAsset>(path);
                if (asset == null) continue;
                Build(asset);
                count++;
            }
            AssetDatabase.Refresh();
            UnityEngine.Debug.Log($"[ExpertSystem] Built {count} rule graph(s).");
        }

        public static void Build(RuleGraphAsset asset)
        {
            if (asset == null) return;
            if (asset.actions == null || asset.actions.Count == 0)
            {
                UnityEngine.Debug.LogWarning($"[ExpertSystem] Skipping '{asset.ruleName}': no actions defined.");
                return;
            }

            EnsureGeneratedDir();

            var className = MakeClassName(asset.ruleName);
            var code = Generate(asset, className);
            var path = Path.Combine(GeneratedDir, className + ".cs");
            File.WriteAllText(path, code);
            AssetDatabase.ImportAsset(path);
        }

        private static string Generate(RuleGraphAsset asset, string className)
        {
            var factTypes = CollectFactTypes(asset);

            var sb = new StringBuilder();
            sb.AppendLine("// Auto-generated from RuleGraphAsset. Do not edit by hand.");
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

            foreach (var t in factTypes)
            {
                sb.AppendLine($"            {t.Name} {VarName(t.Name)} = null!;");
            }
            sb.AppendLine();

            EmitChain(sb, "When()", BuildWhenLines(asset, factTypes));
            sb.AppendLine();
            EmitChain(sb, "Then()", BuildThenLines(asset.actions));

            sb.AppendLine("        }");
            sb.AppendLine("    }");
            sb.AppendLine("}");
            return sb.ToString();
        }

        private static void EmitChain(StringBuilder sb, string head, List<string> lines)
        {
            sb.AppendLine($"            {head}");
            if (lines.Count == 0)
            {
                return;
            }
            for (int i = 0; i < lines.Count; i++)
            {
                var suffix = (i == lines.Count - 1) ? ";" : string.Empty;
                sb.AppendLine($"                {lines[i]}{suffix}");
            }
        }

        private static List<Type> CollectFactTypes(RuleGraphAsset asset)
        {
            var types = new List<Type>();
            var seen = new HashSet<string>();

            void TryAdd(string typeName)
            {
                if (string.IsNullOrEmpty(typeName)) return;
                if (!seen.Add(typeName)) return;
                var t = FactTypeRegistry.FactTypes.FirstOrDefault(x => x.Name == typeName);
                if (t != null) types.Add(t);
            }

            if (asset.conditions != null)
            {
                foreach (var c in asset.conditions) TryAdd(c.factTypeName);
            }

            TryAdd("GameDecision");

            return types;
        }

        private static List<string> BuildWhenLines(RuleGraphAsset asset, List<Type> factTypes)
        {
            var conditions = (asset.conditions ?? new List<ConditionData>())
                .Where(c => !string.IsNullOrEmpty(c.factTypeName) && !string.IsNullOrEmpty(c.fieldName))
                .ToList();
            var logicNodes = asset.logicNodes ?? new List<LogicNodeData>();

            var conditionsByParent = conditions
                .GroupBy(c => c.parentId ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.ToList());

            var logicByParent = logicNodes
                .GroupBy(l => l.parentId ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.ToList());

            var lines = new List<string>();
            var topMatched = new HashSet<string>();

            if (conditionsByParent.TryGetValue(string.Empty, out var topConds))
            {
                var byFact = topConds.GroupBy(c => c.factTypeName).ToDictionary(g => g.Key, g => g.ToList());
                foreach (var t in factTypes)
                {
                    if (!byFact.TryGetValue(t.Name, out var list) || list.Count == 0) continue;
                    var v = VarName(t.Name);
                    var pred = string.Join(" && ", list.Select(c =>
                        $"{v}.{c.fieldName} {OpToCode(c.op)} {FormatValue(t, c.fieldName, c.constantValue)}"));
                    lines.Add($".Match(() => {v}, {v} => {pred})");
                    topMatched.Add(t.Name);
                }
            }

            if (logicByParent.TryGetValue(string.Empty, out var topLogic))
            {
                foreach (var l in topLogic)
                {
                    var grp = EmitLogicGroup(l, conditionsByParent, logicByParent, factTypes, 1);
                    if (!string.IsNullOrEmpty(grp)) lines.Add("." + grp);
                }
            }

            var factsInGroups = CollectFactsInGroups(conditions, logicNodes);
            foreach (var t in factTypes)
            {
                if (topMatched.Contains(t.Name)) continue;
                if (factsInGroups.Contains(t.Name)) continue;
                lines.Add($".Match(() => {VarName(t.Name)})");
            }

            return lines;
        }

        private static HashSet<string> CollectFactsInGroups(List<ConditionData> conditions, List<LogicNodeData> logicNodes)
        {
            var logicIds = new HashSet<string>(logicNodes.Select(l => l.id));
            var result = new HashSet<string>();
            foreach (var c in conditions)
            {
                if (!string.IsNullOrEmpty(c.parentId) && logicIds.Contains(c.parentId))
                {
                    result.Add(c.factTypeName);
                }
            }
            return result;
        }

        private static string EmitLogicGroup(
            LogicNodeData node,
            Dictionary<string, List<ConditionData>> conditionsByParent,
            Dictionary<string, List<LogicNodeData>> logicByParent,
            List<Type> factTypes,
            int depth)
        {
            var childConditions = conditionsByParent.TryGetValue(node.id, out var cl) ? cl : new List<ConditionData>();
            var childLogic = logicByParent.TryGetValue(node.id, out var ll) ? ll : new List<LogicNodeData>();

            switch (node.kind)
            {
                case LogicKind.Not:
                {
                    // NRules Not<T>(predicates...) does not accept an alias.
                    // For MVP we only support NOT wrapping a single condition.
                    if (childConditions.Count == 1 && childLogic.Count == 0)
                    {
                        var c = childConditions[0];
                        var t = factTypes.FirstOrDefault(x => x.Name == c.factTypeName);
                        if (t == null) return null;
                        var p = VarName(t.Name);
                        var pred = $"{p}.{c.fieldName} {OpToCode(c.op)} {FormatValue(t, c.fieldName, c.constantValue)}";
                        return $"Not<{t.Name}>({p} => {pred})";
                    }
                    UnityEngine.Debug.LogWarning(
                        "[ExpertSystem] NOT logic node must wrap exactly one condition; skipping.");
                    return null;
                }
                case LogicKind.And:
                case LogicKind.Or:
                {
                    // NRules 1.0+ uses single-builder syntax:
                    //   .Or(b => b.Match(...).Match(...))   -- branches chained inside builder
                    //   .And(b => b.Match(...).Match(...))  -- patterns chained inside builder
                    var op = node.kind == LogicKind.And ? "And" : "Or";
                    var b = "b" + depth;
                    var calls = new List<string>();

                    if (node.kind == LogicKind.And)
                    {
                        // Inside AND group, conditions on the same fact merge into one Match
                        // so the var binds once with conjunction predicate.
                        var byFact = childConditions
                            .GroupBy(c => c.factTypeName)
                            .ToDictionary(g => g.Key, g => g.ToList());
                        foreach (var t in factTypes)
                        {
                            if (!byFact.TryGetValue(t.Name, out var list) || list.Count == 0) continue;
                            var v = VarName(t.Name);
                            var pred = string.Join(" && ", list.Select(c =>
                                $"{v}.{c.fieldName} {OpToCode(c.op)} {FormatValue(t, c.fieldName, c.constantValue)}"));
                            calls.Add($".Match<{t.Name}>(() => {v}, {v} => {pred})");
                        }
                    }
                    else
                    {
                        // Inside OR group, every condition is its own branch (separate Match call).
                        foreach (var c in childConditions)
                        {
                            var t = factTypes.FirstOrDefault(x => x.Name == c.factTypeName);
                            if (t == null) continue;
                            var v = VarName(t.Name);
                            var pred = $"{v}.{c.fieldName} {OpToCode(c.op)} {FormatValue(t, c.fieldName, c.constantValue)}";
                            calls.Add($".Match<{t.Name}>(() => {v}, {v} => {pred})");
                        }
                    }

                    foreach (var sub in childLogic)
                    {
                        var subEmit = EmitLogicGroup(sub, conditionsByParent, logicByParent, factTypes, depth + 1);
                        if (!string.IsNullOrEmpty(subEmit))
                        {
                            calls.Add("." + subEmit);
                        }
                    }

                    if (calls.Count == 0) return null;
                    return $"{op}({b} => {b}{string.Concat(calls)})";
                }
            }
            return null;
        }

        private static List<string> BuildThenLines(List<ActionData> actions)
        {
            var lines = new List<string>();
            foreach (var a in actions)
            {
                switch (a.kind)
                {
                    case ActionKind.Suggest:
                        lines.Add($".Do(ctx => DecisionMutations.Suggest(decision, TacticalAction.{a.tacticalAction}, {a.utilityScore}, \"{Escape(a.reason)}\", ctx.Rule.Name))");
                        break;
                    case ActionKind.SetStatus:
                        lines.Add($".Do(ctx => DecisionMutations.SetStatus(decision, DecisionStatus.{a.decisionStatus}, ctx.Rule.Name))");
                        break;
                    case ActionKind.Info:
                        lines.Add($".Do(ctx => ctx.Info(decision, \"{Escape(a.message)}\"))");
                        break;
                    case ActionKind.Warning:
                        lines.Add($".Do(ctx => ctx.Warning(decision, \"{Escape(a.message)}\"))");
                        break;
                    case ActionKind.Error:
                        lines.Add($".Do(ctx => ctx.Error(decision, \"{Escape(a.message)}\"))");
                        break;
                    case ActionKind.Update:
                        lines.Add(".Do(ctx => ctx.Update(decision))");
                        break;
                }
            }
            return lines;
        }

        private static string FormatValue(Type factType, string fieldName, string raw)
        {
            var member = FactTypeRegistry.GetMembers(factType).FirstOrDefault(m => m.Name == fieldName);
            var memberType = member != null ? FactTypeRegistry.GetMemberType(member) : null;
            raw = raw ?? string.Empty;

            if (memberType == typeof(string))
                return $"\"{Escape(raw)}\"";
            if (memberType == typeof(bool))
                return raw.Trim().Equals("true", StringComparison.OrdinalIgnoreCase) ? "true" : "false";
            if (memberType != null && memberType.IsEnum)
                return $"{memberType.Name}.{raw}";
            if (memberType == typeof(float) || memberType == typeof(double))
                return raw.EndsWith("f", StringComparison.OrdinalIgnoreCase) ? raw : raw + "f";
            return string.IsNullOrEmpty(raw) ? "0" : raw;
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

        private static string VarName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName)) return "fact";
            if (typeName == "GameDecision") return "decision";
            return char.ToLowerInvariant(typeName[0]) + typeName.Substring(1);
        }

        private static string OpToCode(ComparisonOperator op) => op switch
        {
            ComparisonOperator.Equal => "==",
            ComparisonOperator.NotEqual => "!=",
            ComparisonOperator.Less => "<",
            ComparisonOperator.LessOrEqual => "<=",
            ComparisonOperator.Greater => ">",
            ComparisonOperator.GreaterOrEqual => ">=",
            _ => "=="
        };

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
