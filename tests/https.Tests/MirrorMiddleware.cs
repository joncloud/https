using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Https.Tests
{
    class MirrorMiddleware
    {
        readonly RequestDelegate _next;
        public MirrorMiddleware(RequestDelegate next) =>
            _next = next;
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Mirror"))
            {
                if (!string.IsNullOrEmpty(context.Request.ContentType))
                {
                    context.Response.ContentType = context.Request.ContentType;
                }

                await context.Request.Body.CopyToAsync(context.Response.Body);
            }
            else
            {
                await _next(context);
            }
        }
    }
}
