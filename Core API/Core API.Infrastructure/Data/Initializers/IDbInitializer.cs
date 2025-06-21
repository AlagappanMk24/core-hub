namespace Core_API.Infrastructure.Data.Initializers
{
    public interface IDbInitializer
    {
        Task Initialize(CancellationToken cancellationToken);
    }
}