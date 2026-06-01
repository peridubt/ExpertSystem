// Факт-профиль действующего персонажа: роль и характеристики, влияющие на ролевые правила.
namespace ExpertSystem.RuleEngine.Core.Domain
{
    /// <summary>Роль и параметры самого игрока (актора).</summary>
    public class CharacterProfile
    {
        public string ActorId { get; set; } = string.Empty;
        public CharacterRole Role { get; set; } = CharacterRole.Unspecified;
        public RiskTolerance RiskTolerance { get; set; } = RiskTolerance.Balanced;
        public int Level { get; set; } = 1;
    }
}
