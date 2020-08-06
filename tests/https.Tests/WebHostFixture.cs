using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Https.Tests
{
    public class WebHostFixture : IDisposable
    {
        readonly IWebHost _webHost;
        readonly Task _task;
        readonly CancellationTokenSource _cts;
        public string HttpUrl { get; }
        public string HttpsUrl { get; }

        public WebHostFixture()
        {
            _webHost = WebHost.CreateDefaultBuilder()
                .UseUrls(
                    HttpUrl = "http://localhost:5000", 
                    HttpsUrl = "https://localhost:5001"
                )
                .UseStartup<Startup>()
                .Build();
            _cts = new CancellationTokenSource();
            _task = _webHost.RunAsync(_cts.Token);
        }

        public async void Dispose()
        {
            await _webHost.StopAsync();
            _cts.Cancel();
            _cts.Dispose();
        }
    }
}
