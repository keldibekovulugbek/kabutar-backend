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

// Load .env file
DotNetEnv.Env.Load();

// Override appsettings with environment variables
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

// Configure Serilog
builder.ConfigureLogger();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.AddDataAccessLayer();
builder.AddServiceLayer();
builder.AddApiLayer();





//-> Middlewares
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Log all incoming requests
app.Use(async (context, next) =>
{
    var requestTime = DateTime.UtcNow;
    Log.Information("📥 Incoming Request: {Method} {Path} from {IP}",
        context.Request.Method,
        context.Request.Path,
        context.Connection.RemoteIpAddress);

    await next.Invoke();

    var duration = DateTime.UtcNow - requestTime;
    var statusCode = context.Response.StatusCode;

    if (statusCode >= 200 && statusCode < 300)
        Log.Information("✅ Response Sent: {StatusCode} for {Method} {Path} in {Duration}ms",
            statusCode, context.Request.Method, context.Request.Path, duration.TotalMilliseconds);
    else if (statusCode >= 400 && statusCode < 500)
        Log.Warning("⚠️ Client Error: {StatusCode} for {Method} {Path} in {Duration}ms",
            statusCode, context.Request.Method, context.Request.Path, duration.TotalMilliseconds);
    else if (statusCode >= 500)
        Log.Error("❌ Server Error: {StatusCode} for {Method} {Path} in {Duration}ms",
            statusCode, context.Request.Method, context.Request.Path, duration.TotalMilliseconds);
});

app.UseStaticFiles();
app.UseMiddleware<ExceptionHandlerMiddleware>();

// CORS must be before rate limiting
app.UseCors("AllowAll");

// Rate limiting middleware
app.UseIpRateLimiting();

app.MapHub<ChatHub>("/hubs/chat");
// Disabled for development - WPF uses HTTP
// app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
