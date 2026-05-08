using System;
using System.Collections.Generic;
using System.Reflection;
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.Editor.GraphView
{
    public static class FactTypeRegistry
    {
        private static readonly Lazy<List<Type>> CachedFactTypes = new Lazy<List<Type>>(DiscoverFactTypes);

        public static IReadOnlyList<Type> FactTypes => CachedFactTypes.Value;

        public static IReadOnlyList<MemberInfo> GetMembers(Type factType)
        {
            if (factType == null) return Array.Empty<MemberInfo>();
            var members = new List<MemberInfo>();
            foreach (var prop in factType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetIndexParameters().Length > 0) continue;
                if (IsLeaf(prop.PropertyType)) members.Add(prop);
            }
            foreach (var field in factType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (IsLeaf(field.FieldType)) members.Add(field);
            }
            members.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            return members;
        }

        public static Type GetMemberType(MemberInfo member)
        {
            return member switch
            {
                PropertyInfo p => p.PropertyType,
                FieldInfo f => f.FieldType,
                _ => null
            };
        }

        private static List<Type> DiscoverFactTypes()
        {
            var assembly = typeof(GameDecision).Assembly;
            var ns = typeof(GameDecision).Namespace;
            var result = new List<Type>();
            foreach (var t in assembly.GetTypes())
            {
                if (t.Namespace != ns) continue;
                if (!t.IsClass || t.IsAbstract) continue;
                result.Add(t);
            }
            result.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            return result;
        }

        private static bool IsLeaf(Type t)
        {
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal);
        }
    }
}
