namespace Kabutar.Api.Configurations
{
    public static class CorsPolicyConfiguration
    {
        public static void ConfigureCorsPolicy(this IServiceCollection services)
        {
            // Get allowed origins from environment variable or use defaults
            var allowedOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS")
                ?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                ?? new[] { "http://localhost:3000", "http://localhost:5173", "http://localhost:5000" };

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.WithOrigins(allowedOrigins)
                           .AllowAnyMethod()
                           .AllowAnyHeader()
                           .AllowCredentials(); // SignalR needs this
                });
            });
        }
    }
}
