// Реестр типов фактов для редактора графов. По рефлексии находит доменные классы
// и их простые поля/свойства — из них формируются выпадающие списки в узлах условий.
using System;
using System.Collections.Generic;
using System.Reflection;
using ExpertSystem.RuleEngine.Core.Domain;

namespace ExpertSystem.Editor.GraphView
{
    /// <summary>Обнаружение типов фактов и их членов через рефлексию (с кэшированием).</summary>
    public static class FactTypeRegistry
    {
        private static readonly Lazy<List<Type>> CachedFactTypes = new Lazy<List<Type>>(DiscoverFactTypes);

        /// <summary>Список доступных типов фактов (кэшируется при первом обращении).</summary>
        public static IReadOnlyList<Type> FactTypes => CachedFactTypes.Value;

        /// <summary>
        /// Возвращает простые члены (поля и свойства листовых типов) факта для выбора в UI.
        /// Принимает тип факта, возвращает отсортированный список членов.
        /// </summary>
        public static IReadOnlyList<MemberInfo> GetMembers(Type factType)
        {
            if (factType == null) return Array.Empty<MemberInfo>();
            var members = new List<MemberInfo>();
            foreach (var prop in factType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (prop.GetIndexParameters().Length > 0) continue; // пропускаем индексаторы
                if (IsLeaf(prop.PropertyType)) members.Add(prop);
            }
            foreach (var field in factType.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (IsLeaf(field.FieldType)) members.Add(field);
            }
            members.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            return members;
        }

        /// <summary>Возвращает тип члена (свойства или поля); null для прочего.</summary>
        public static Type GetMemberType(MemberInfo member)
        {
            return member switch
            {
                PropertyInfo p => p.PropertyType,
                FieldInfo f => f.FieldType,
                _ => null
            };
        }

        /// <summary>Находит все классы из пространства имён доменных фактов. Возвращает список типов.</summary>
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

        /// <summary>Признак "листового" типа: примитив, enum, строка или decimal.</summary>
        private static bool IsLeaf(Type t)
        {
            return t.IsPrimitive || t.IsEnum || t == typeof(string) || t == typeof(decimal);
        }
    }
}
