using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TezGel.Domain.Entities;

namespace TezGel.Persistence.Context
{
    public class TezGelDbContext:IdentityDbContext
    {
        public TezGelDbContext(DbContextOptions<TezGelDbContext> options) : base(options)
        {
        }

        public DbSet<TestEntity> TestEntities { get; set; }
    
        
    }
}