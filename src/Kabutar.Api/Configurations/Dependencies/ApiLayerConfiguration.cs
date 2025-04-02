using Kabutar.Api.Notifiers;
using Kabutar.Service.Interfaces.Common;

namespace Kabutar.Api.Configurations.Dependencies
{
    public static class ApiLayerConfiguration
    {
        public static void AddApiLayer(this WebApplicationBuilder builder)
        {
            //builder.ConfigureLogger();
            builder.Services.AddScoped<IChatNotifier, ChatNotifier>();

            builder.Services.ConfigureCorsPolicy();
            builder.Services.AddHttpContextAccessor();
            builder.ConfigureJwt();
            builder.Services.ConfigureSwaggerAuthorize();
        }
    }
}
