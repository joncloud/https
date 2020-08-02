using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Https.Tests
{
    public class Startup
    {   
        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<RedirectMiddleware>();
            app.UseMiddleware<MirrorMiddleware>();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello!");
            });
        }
    }
}
