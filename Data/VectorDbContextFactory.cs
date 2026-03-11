using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DocumentQA.Data
{
    public class VectorDbContextFactory : IDesignTimeDbContextFactory<VectorDbContext>
    {
        public VectorDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false)
                .AddEnvironmentVariables()
                .Build();

            var provider = config["DatabaseProvider"]?.ToLowerInvariant();
            var optionsBuilder = new DbContextOptionsBuilder<VectorDbContext>();

            if (provider == "sqlite")
            {
                optionsBuilder.UseSqlite(config.GetConnectionString("SQLite"));
            }
            else
            {
                throw new InvalidOperationException(
                    "DatabaseProvider must be set to SQLite in appsettings.json");
            }

            return new VectorDbContext(optionsBuilder.Options);
        }
    }
}