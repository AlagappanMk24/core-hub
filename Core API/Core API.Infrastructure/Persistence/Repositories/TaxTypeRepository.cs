﻿using Core_API.Application.Contracts.Persistence;
using Core_API.Domain.Entities;
using Core_API.Infrastructure.Data.Context;

namespace Core_API.Infrastructure.Persistence.Repositories
{
    public class TaxTypeRepository : GenericRepository<TaxType>, ITaxTypeRepository
    {
        public TaxTypeRepository(CoreAPIDbContext dbContext) : base(dbContext)
        {
        }
    }
}
