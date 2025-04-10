using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Models.Entities;
using Core_API.Infrastructure.Data.Context;
using Core_API.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace Core_API.Infrastructure.Persistence.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CoreAPIDbContext _context;
        private readonly IConfiguration _configuration;
        public IUserRepository Users { get; private set; }

        public UnitOfWork(CoreAPIDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            Users = new UserRepository(_context, userManager);
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}