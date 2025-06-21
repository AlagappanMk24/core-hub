using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class UserRepository(CoreAPIDbContext dbContext) : GenericRepository<ApplicationUser>(dbContext), IUserRepository
    {
        private readonly CoreAPIDbContext _dbContext = dbContext;
        public void Update(ApplicationUser applicationUser)
        {
            _dbContext.ApplicationUsers.Update(applicationUser);
        }
        public async Task<bool> UpdateUserAsync(ApplicationUser user)
        {
            try
            {
                // Save changes to the database
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string id)
        {
            return await _dbContext.ApplicationUsers
             .Include(u => u.Company)
             .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
