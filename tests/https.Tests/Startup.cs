using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Https.Tests
{
    public class Startup
    {   
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Run(async (context) =>
            {
                if (!string.IsNullOrEmpty(context.Request.ContentType))
                {
                    context.Response.ContentType = context.Request.ContentType;
                }

                await context.Request.Body.CopyToAsync(context.Response.Body);
            });
        }
    }
}
