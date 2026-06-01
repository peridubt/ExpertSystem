// Общая на весь процесс точка доступа к движку. RETE-сеть дорого компилировать,
// поэтому движок собирается один раз и переиспользуется всеми агентами игры.
using System.Reflection;
using ExpertSystem.RuleEngine.Core.Runtime;
using ExpertSystem.Rules;

namespace ExpertSystem.Integration
{
    /// <summary>Кэш общего движка правил для всех компонентов ExpertSystemBrain.</summary>
    public static class ExpertSystemRuntime
    {
        private static readonly object Gate = new object();
        private static GameRulesEngine _engine;
        private static Assembly[] _ruleAssemblies;

        /// <summary>
        /// Задаёт сборки, в которых искать правила. Вызывается один раз при старте,
        /// до первого вывода. Без аргументов — возврат к сборке правил по умолчанию.
        /// Сбрасывает кэш, движок пересоберётся при следующем обращении.
        /// </summary>
        public static void Configure(params Assembly[] ruleAssemblies)
        {
            lock (Gate)
            {
                _ruleAssemblies = (ruleAssemblies != null && ruleAssemblies.Length > 0)
                    ? ruleAssemblies
                    : null;
                _engine = null;
            }
        }

        /// <summary>
        /// Общий движок (ленивая компиляция). Первое обращение собирает RETE-сеть,
        /// последующие возвращают кэш. Потокобезопасно.
        /// </summary>
        public static GameRulesEngine Engine
        {
            get
            {
                if (_engine != null) return _engine;
                lock (Gate)
                {
                    if (_engine == null)
                    {
                        // По умолчанию — сборка правил, найденная через маркер.
                        var assemblies = _ruleAssemblies
                            ?? new[] { typeof(RulesAssemblyMarker).Assembly };
                        _engine = new GameRulesEngine(assemblies);
                    }
                }
                return _engine;
            }
        }

        /// <summary>
        /// Сбрасывает кэш движка и настройки. Следующее обращение к Engine пересоберёт
        /// сеть с нуля. Полезно после регенерации правил или для изоляции тестов.
        /// </summary>
        public static void Reset()
        {
            lock (Gate)
            {
                _engine = null;
                _ruleAssemblies = null;
            }
        }
    }
}
