using AspNetCoreRateLimit;

namespace Kabutar.Api.Configurations
{
    public static class RateLimitConfig
    {
        public static void ConfigureRateLimiting(this IServiceCollection services)
        {
            services.AddMemoryCache();

            services.Configure<IpRateLimitOptions>(options =>
            {
                options.GeneralRules = new List<RateLimitRule>
                {
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Period = "1m",
                        Limit = 60
                    },
                    new RateLimitRule
                    {
                        Endpoint = "*",
                        Period = "1h",
                        Limit = 1000
                    }
                };

                options.EndpointWhitelist = new List<string>();

                options.RateLimitCounterPrefix = "kabutar_rl";

                options.HttpStatusCode = 429;

                options.QuotaExceededMessage = "Too many requests. Please try again later.";
            });

            services.Configure<IpRateLimitPolicies>(options =>
            {
                options.IpRules = new List<IpRateLimitPolicy>
                {
                    new IpRateLimitPolicy
                    {
                        Ip = "*",
                        Rules = new List<RateLimitRule>
                        {
                            new RateLimitRule
                            {
                                Endpoint = "POST:/api/account/login",
                                Period = "1m",
                                Limit = 5
                            },
                            new RateLimitRule
                            {
                                Endpoint = "POST:/api/account/register",
                                Period = "1m",
                                Limit = 3
                            },
                            new RateLimitRule
                            {
                                Endpoint = "POST:/api/account/send-code",
                                Period = "1m",
                                Limit = 2
                            },
                            new RateLimitRule
                            {
                                Endpoint = "POST:/api/account/reset-password",
                                Period = "5m",
                                Limit = 3
                            },
                            new RateLimitRule
                            {
                                Endpoint = "POST:/api/messages",
                                Period = "1m",
                                Limit = 60
                            },
                            new RateLimitRule
                            {
                                Endpoint = "POST:/api/messages/text",
                                Period = "1m",
                                Limit = 60
                            },
                            new RateLimitRule
                            {
                                Endpoint = "DELETE:/api/messages",
                                Period = "1m",
                                Limit = 30
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
