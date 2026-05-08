namespace ExpertSystem.Rules.Graph
{
    public enum ComparisonOperator
    {
        Equal,
        NotEqual,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual,
    }

    public enum ActionKind
    {
        Suggest,
        SetStatus,
        Info,
        Warning,
        Error,
        Update,
    }

    public enum LogicKind
    {
        And,
        Or,
        Not,
    }
}
