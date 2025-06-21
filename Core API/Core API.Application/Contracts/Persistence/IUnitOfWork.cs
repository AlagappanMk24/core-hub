namespace Core_API.Application.Contracts.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IAuthRepository AuthUsers {  get; }
        IUserRepository Users { get; }
        Task SaveChangesAsync();
    }
}