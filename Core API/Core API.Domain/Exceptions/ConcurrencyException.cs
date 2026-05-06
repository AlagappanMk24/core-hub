namespace Core_API.Domain.Exceptions
{

    /// <summary>
    /// Exception thrown when concurrency conflict occurs.
    /// </summary>
    public sealed class ConcurrencyException : DomainException
    {
        public string EntityName { get; }
        public int EntityId { get; }

        public ConcurrencyException(string entityName, int entityId)
            : base($"Concurrency conflict occurred while updating '{entityName}' with ID '{entityId}'. The entity was modified or deleted by another process.")
        {
            EntityName = entityName;
            EntityId = entityId;
        }
    }
}