namespace Core_API.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when a business rule is violated.
    /// </summary>
    public sealed class BusinessRuleViolationException : DomainException
    {
        public string RuleName { get; }
        public string? EntityName { get; }

        public BusinessRuleViolationException(string ruleName, string message)
            : base(message)
        {
            RuleName = ruleName;
        }

        public BusinessRuleViolationException(string ruleName, string entityName, string message)
            : base(message)
        {
            RuleName = ruleName;
            EntityName = entityName;
        }
    }
}