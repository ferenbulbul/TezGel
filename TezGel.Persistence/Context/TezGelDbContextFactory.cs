using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;


namespace TezGel.Persistence.Context
{
    public class TezGelDbContextFactory : IDesignTimeDbContextFactory<TezGelDbContext>
    {
        public TezGelDbContext CreateDbContext(string[] args)
        {
             IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.Development.json")
                .Build();

            var builder = new DbContextOptionsBuilder<TezGelDbContext>();
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            builder.UseNpgsql(connectionString);

            return new TezGelDbContext(builder.Options);
        }
    }
}