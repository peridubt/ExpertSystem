// Маркер сборки правил. Нужен только для получения ссылки на эту сборку через
// рефлексию (typeof(RulesAssemblyMarker).Assembly), чтобы движок нашёл и скомпилировал
// все правила без привязки к конкретному классу правила.
namespace ExpertSystem.Rules
{
    /// <summary>Пустой тип-маркер для доступа к сборке правил.</summary>
    public sealed class RulesAssemblyMarker
    {
        private RulesAssemblyMarker()
        {
        }
    }
}
