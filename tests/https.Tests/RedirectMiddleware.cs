using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Https.Tests
{
    class RedirectMiddleware
    {
        readonly RequestDelegate _next;
        public RedirectMiddleware(RequestDelegate next) =>
            _next = next;
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Redirect"))
            {
                context.Response.StatusCode = StatusCodes.Status301MovedPermanently;
                context.Response.Headers.Add("Location", "http://localhost:5000/Mirror");
            }
            else
            {
                await _next(context);
            }
        }
    }
}
