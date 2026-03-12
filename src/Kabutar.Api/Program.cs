using AspNetCoreRateLimit;
using Kabutar.Api.Configurations;
using Kabutar.Api.Configurations.Dependencies;
using Kabutar.Api.Hubs;
using Kabutar.Api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


DotNetEnv.Env.Load();


builder.Configuration["ConnectionStrings:DefaultConnection"] =
    $"Host={Environment.GetEnvironmentVariable("DB_HOST") ?? "localhost"};" +
    $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"};" +
    $"Database={Environment.GetEnvironmentVariable("DB_NAME") ?? "kabutar-db"};" +
    $"Username={Environment.GetEnvironmentVariable("DB_USER") ?? "postgres"};" +
    $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")}";

builder.Configuration["Jwt:Key"] = Environment.GetEnvironmentVariable("JWT_KEY");
builder.Configuration["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER");
builder.Configuration["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
builder.Configuration["Jwt:Lifetime"] = Environment.GetEnvironmentVariable("JWT_LIFETIME") ?? "300";

builder.Configuration["Email:Host"] = Environment.GetEnvironmentVariable("EMAIL_HOST") ?? "smtp.gmail.com";
builder.Configuration["Email:EmailAddress"] = Environment.GetEnvironmentVariable("EMAIL_ADDRESS");
builder.Configuration["Email:Password"] = Environment.GetEnvironmentVariable("EMAIL_PASSWORD");

builder.Configuration["Serilog:WriteTo:1:Args:Token"] = Environment.GetEnvironmentVariable("TELEGRAM_BOT_TOKEN");
builder.Configuration["Serilog:WriteTo:1:Args:ChatId"] = Environment.GetEnvironmentVariable("TELEGRAM_CHAT_ID");

builder.Configuration["ENCRYPTION_KEY"] = Environment.GetEnvironmentVariable("ENCRYPTION_KEY");


builder.ConfigureLogger();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddDataAccessLayer();
builder.AddServiceLayer();
builder.AddApiLayer();


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseStaticFiles();
app.UseMiddleware<ExceptionHandlerMiddleware>();


app.UseCors("AllowAll");


app.UseIpRateLimiting();

app.MapHub<ChatHub>("/hubs/chat");


app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
