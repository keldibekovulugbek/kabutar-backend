

using Kabutar.DataAccess.Interfaces;
using Kabutar.DataAccess.Interfaces.Messages;
using Kabutar.DataAccess.Interfaces.Users;
using Kabutar.DataAccess.Repositories.Messages;
using Kabutar.DataAccess.Repositories.Users;
using Kabutar.Service.Interfaces.Accounts;
using Kabutar.Service.Interfaces.Common;
using Kabutar.Service.Interfaces.Users;
using Kabutar.Service.Managers;
using Kabutar.Service.Security;
using Kabutar.Service.Services.Accounts;
using Kabutar.Service.Services.Common;
using Kabutar.Service.Services.Users;

namespace Kabutar.Api.Configurations.Dependencies
{
    public static class ServiceLayerConfiguration
    {
        public static void AddServiceLayer(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IMessageRepository, MessageRepository>();
            builder.Services.AddScoped<IFileService, FileService>();
            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<IAuthManager, AuthManager>();
            builder.Services.AddScoped<IAccountService, AccountService>();
            builder.Services.AddScoped<IIdentityHelperService, IdentityHelperService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddMemoryCache();
        }
    }
}
