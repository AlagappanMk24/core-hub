using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities.Identity;
using Core_API.Infrastructure.Data.Context;
using Core_API.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Core_API.Infrastructure.Persistence
{
    public class UnitOfWork(CoreAPIDbContext dbContext, UserManager<ApplicationUser> userManager) : IUnitOfWork
    {
        private readonly CoreAPIDbContext _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        private readonly UserManager<ApplicationUser> _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

        // Private fields to hold instances of repositories
        private IAuthRepository _authUsers;
        private IUserRepository _users;
        public IAuthRepository AuthUsers
        {
            get
            {
                if (_authUsers == null)
                {
                    _authUsers = new AuthRepository(_dbContext, _userManager);
                }
                return _authUsers;
            }
        }
        public IUserRepository Users
        {
            get
            {
                if (_users == null)
                {
                    _users = new UserRepository(_dbContext);
                }
                return _users;
            }
        }
        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }

        // Implement IDisposable for proper resource management
        public void Dispose()
        {
            _dbContext?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}