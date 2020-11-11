using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading.Tasks;

namespace Https.Tests
{
    class TimeoutMiddleware
    {
        readonly RequestDelegate _next;
        public TimeoutMiddleware(RequestDelegate next) =>
            _next = next;
        
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Timeout") &&
                context.Request.Query.TryGetValue("delay", out var delayValues) &&
                int.TryParse(delayValues.FirstOrDefault(), out var delay))
            {
                await Task.Delay(delay);
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync("Ignore this");
            }
            else
            {
                await _next(context);
            }
        }
    }
}
