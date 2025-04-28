using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TezGel.Domain.Entities;


namespace TezGel.Persistence.Context
{
    public class TezGelDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public TezGelDbContext(DbContextOptions<TezGelDbContext> options) : base(options)
        {
        }

        public DbSet<CustomerUser> CustomerUsers { get; set; }
        public DbSet<BusinessUser> BusinessUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // CustomerUser mapping
            builder.Entity<CustomerUser>(entity =>
            {
                entity.ToTable("CustomerUsers");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.AppUser)
                      .WithOne(u => u.CustomerUser)
                      .HasForeignKey<CustomerUser>(e => e.Id);
            });

            // BusinessUser mapping
            builder.Entity<BusinessUser>(entity =>
            {
                entity.ToTable("BusinessUsers");
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.AppUser)
                      .WithOne(u => u.BusinessUser)
                      .HasForeignKey<BusinessUser>(e => e.Id);
            });
        }


    }
}