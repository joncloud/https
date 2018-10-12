using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Https.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var webHost = WebHost.CreateDefaultBuilder()
                .UseUrls("http://localhost:5000")
                .UseStartup<Startup>()
                .Build();

            var task = webHost.RunAsync();

            var args = new[]
            {
                "post", "http://localhost:5000", "--json", "foo=bar", "lorem=ipsum"
            };

            using (var stdin = new MemoryStream())
            using (var stdout = new MemoryStream())
            using (var stderr = new MemoryStream())
            {
                await new Program(() => stderr, () => stdin, () => stdout, false)
                    .RunAsync(args);

                stdout.Position = 0;
                var json = new StreamReader(stdout).ReadToEnd();
                Assert.Equal("{\"foo\":\"bar\",\"lorem\":\"ipsum\"}", json);
                stderr.Position = 0;
            }

            

            await webHost.StopAsync();
        }
    }

    public class Startup
    {   
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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
