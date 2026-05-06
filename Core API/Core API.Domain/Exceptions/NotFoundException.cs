namespace Core_API.Domain.Exceptions
{
    /// <summary>
    /// Exception thrown when an entity is not found.
    /// </summary>
    public sealed class NotFoundException : DomainException
    {
        public string EntityName { get; }
        public object? EntityId { get; }

        public NotFoundException(string entityName, object entityId)
            : base($"Entity '{entityName}' with identifier '{entityId}' was not found.")
        {
            EntityName = entityName;
            EntityId = entityId;
        }

        public NotFoundException(string entityName, object entityId, string message)
            : base(message)
        {
            EntityName = entityName;
            EntityId = entityId;
        }
    }
}