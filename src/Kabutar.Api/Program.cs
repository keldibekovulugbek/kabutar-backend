using Kabutar.Api.Configurations.Dependencies;
using Kabutar.Api.Hubs;
using Kabutar.Api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
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

/*app.Use(async (context, next) =>
{
    Log.Information("Received Request: {Method} {Path}", context.Request.Method, context.Request.Path);
    await next.Invoke();
    Log.Information("Response Sent: {StatusCode}", context.Response.StatusCode);
});
*/

app.UseStaticFiles();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseCors("AllowAll");
app.MapHub<ChatHub>("/hubs/chat");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
