﻿namespace Kabutar.Api.Configurations.Dependencies
{
    public static class ApiLayerConfiguration
    {
        public static void AddApiLayer(this WebApplicationBuilder builder)
        {
            builder.ConfigureLogger();
            builder.Services.ConfigureCorsPolicy();
            builder.Services.AddHttpContextAccessor();
            builder.ConfigureJwt();
            builder.Services.ConfigureSwaggerAuthorize();
        }
    }
}
