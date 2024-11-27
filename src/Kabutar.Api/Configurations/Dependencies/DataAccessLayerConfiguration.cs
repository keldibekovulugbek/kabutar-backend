using Kabutar.DataAccess;
using Kabutar.DataAccess.Context;
using Kabutar.DataAccess.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Kabutar.Api.Configurations.Dependencies
{
    public static class DataAccessLayerConfiguration
    {
        public static void AddDataAccessLayer(this WebApplicationBuilder builder)
        {
            string connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(connectionString);
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });

            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        }
    }
}
