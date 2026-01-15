using Kabutar.Service.DTOs.Common;
using Kabutar.Service.Exceptions;
using Newtonsoft.Json;
using Serilog;

namespace   Kabutar.Api.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebHostEnvironment _env;

        public ExceptionHandlerMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            this._next = next;
            this._env = env;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (StatusCodeException exception)
            {
                await ClientErrorHandleAsync(httpContext, exception);
            }
            catch (Exception exception)
            {
                await SystemErrorHandleAsync(httpContext, exception);
            }
        }

        public async Task ClientErrorHandleAsync(HttpContext httpContext, StatusCodeException exception)
        {
            // Log client errors (400-level)
            Log.Warning("⚠️ Client Error on {Method} {Path}: {StatusCode} - {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                (int)exception.HttpStatusCode,
                exception.Message);

            httpContext.Response.ContentType = "application/json";
            ErrorResponse result = new()
            {
                Message = exception.Message,
                StatusCode = (int)exception.HttpStatusCode
            };
            httpContext.Response.StatusCode = (int)exception.HttpStatusCode;
            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }

        public async Task SystemErrorHandleAsync(HttpContext httpContext, Exception exception)
        {
            // Log server errors (500-level) with full stack trace
            Log.Error(exception, "❌ Server Error on {Method} {Path}: {Message}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message);

            httpContext.Response.ContentType = "application/json";
            ErrorResponse result = new();
            if (_env.IsProduction())
            {
                result.Message = exception.Message;
                result.StatusCode = 500;
            }
            else
            {
                result.Message = exception.ToString();
                result.StatusCode = 500;
            }
            httpContext.Response.StatusCode = 500;
            await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(result));
        }
    }
}
