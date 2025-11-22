using Core_API.Domain.Entities.Identity;

namespace Core_API.Application.Contracts.Persistence
{
    public interface IUserRepository : IGenericRepository<ApplicationUser>
    {
        new void Update(ApplicationUser applicationUser);
        Task<bool> UpdateUserAsync(ApplicationUser user);
        Task<ApplicationUser> GetUserByIdAsync(string id);
    }
}