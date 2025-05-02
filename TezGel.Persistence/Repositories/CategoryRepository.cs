using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Domain.Entities;
using TezGel.Persistence.Context;

namespace TezGel.Persistence.Repositories
{
    public class CategoryRepository:GenericRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(TezGelDbContext context) : base(context)
        {
        }
        
    }
}