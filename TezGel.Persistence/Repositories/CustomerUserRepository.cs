using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TezGel.Application.Interfaces.Repositories;
using TezGel.Domain.Entities;
using TezGel.Persistence.Context;

namespace TezGel.Persistence.Repositories
{
     public class CustomerUserRepository : GenericRepository<CustomerUser>, ICustomerUserRepository
    {
        public CustomerUserRepository(TezGelDbContext context) : base(context)
        {
        }

        // CustomerUser'a özel metodlar buraya gelebilir.
    }
}