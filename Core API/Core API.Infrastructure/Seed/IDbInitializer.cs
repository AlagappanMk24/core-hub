namespace Core_API.Infrastructure.Seed
{
    public interface IDbInitializer
    {
        Task Initialize(CancellationToken cancellationToken);
    }
}