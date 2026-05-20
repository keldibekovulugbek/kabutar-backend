using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Kabutar.DataAccess.Context
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "../Kabutar.Api");

            var envPath = Path.Combine(basePath, ".env");
            if (File.Exists(envPath))
            {
                foreach (var rawLine in File.ReadAllLines(envPath))
                {
                    var line = rawLine.Trim();
                    if (line.Length == 0 || line.StartsWith("#")) continue;
                    var eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    var key = line.Substring(0, eq).Trim();
                    var value = line.Substring(eq + 1).Trim();
                    if (Environment.GetEnvironmentVariable(key) == null)
                        Environment.SetEnvironmentVariable(key, value);
                }
            }

            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .Build();

            var connectionString = config.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                connectionString =
                    $"Host={Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"};" +
                    $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"};" +
                    $"Database={Environment.GetEnvironmentVariable("DB_NAME") ?? "kabutar-db"};" +
                    $"Username={Environment.GetEnvironmentVariable("DB_USER") ?? "postgres"};" +
                    $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";
            }

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
