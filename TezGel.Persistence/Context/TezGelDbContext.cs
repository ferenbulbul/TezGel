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
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }

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
            builder.Entity<Product>(entity =>
            {
                entity.ToTable("Products");
                entity.HasKey(p => p.Id);


                entity.HasOne(p => p.BusinessUser)
                      .WithMany()
                      .HasForeignKey(p => p.BusinessUserId);

                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId);
            });

            builder.Entity<Category>(entity =>
            {
                entity.ToTable("Categories");
                entity.HasKey(c => c.Id);
            });

            builder.Entity<Category>().HasData(
            new Category
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Tatlı",
                Description = "Tatlı ve şekerli ürünler",
                CreatedDate = DateTime.UtcNow
            },
             new Category
             {
                 Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                 Name = "Hamur İşi",
                 Description = "Poğaça, börek, çörek",
                 CreatedDate = DateTime.UtcNow
             },
            new Category
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "İçecek",
                Description = "Soğuk ve sıcak içecekler",
                CreatedDate = DateTime.UtcNow
            }
);
        }
    }
}