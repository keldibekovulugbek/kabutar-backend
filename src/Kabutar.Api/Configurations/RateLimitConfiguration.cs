using AspNetCoreRateLimit;

namespace Kabutar.Api.Configurations
{
    public static class RateLimitConfig
    {
        public static void ConfigureRateLimiting(this IServiceCollection services)
        {
            // Rate limit configuration
            services.AddMemoryCache();

            services.Configure<IpRateLimitOptions>(options =>
            {
                // General rules for all endpoints
                options.GeneralRules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Period = "1m",
                        Limit = 60 // 60 requests per minute per IP
                    },
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Period = "1h",
                        Limit = 1000 // 1000 requests per hour per IP
                    }
                };

                // Specific rules for sensitive endpoints
                options.EndpointWhitelist = new List<string>();

                // Rate limit counter prefix
                options.RateLimitCounterPrefix = "kabutar_rl";

                // HTTP status code returned when rate limit is exceeded
                options.HttpStatusCode = 429;

                // Message when rate limit exceeded
                options.QuotaExceededMessage = "Too many requests. Please try again later.";
            });

            services.Configure<IpRateLimitPolicies>(options =>
            {
                options.IpRules = new List<IpRateLimitPolicy>
                {
                    // Stricter limits for auth endpoints
                    new IpRateLimitPolicy
                    {
                        Ip = "*",
                        Rules = new List<RateLimitRule>
                        {
                            new RateLimitRule
                            {
                                Endpoint = "POST:/api/account/login",
                                Period = "1m",
                                Limit = 5 // Only 5 login attempts per minute
                            },
                            new RateLimitRule
                            {
                                Endpoint = "POST:/api/account/register",
                                Period = "1m",
                                Limit = 3 // 3 registration attempts per minute
                            },
                            new RateLimitRule
                            {
                                Endpoint = "POST:/api/account/send-code",
                                Period = "1m",
                                Limit = 2 // 2 code requests per minute
                            },
                            new RateLimitRule
                            {
                                Endpoint = "POST:/api/account/reset-password",
                                Period = "5m",
                                Limit = 3 // 3 password resets per 5 minutes
                            }
                        }
                    }
                };
            });

            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
        }
    }
}
