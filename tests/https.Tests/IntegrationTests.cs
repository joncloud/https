using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Https.Tests
{
    public class IntegrationTests : IClassFixture<WebHostFixture>
    {
        readonly WebHostFixture _fixture;
        public IntegrationTests(WebHostFixture fixture) =>
            _fixture = fixture;

        [Fact]
        public async Task MirrorTests()
        {
            var args = new[]
            {
                "post", $"{_fixture.Url}/Mirror", "--json", "foo=bar", "lorem=ipsum"
            };

            var result = await Https.ExecuteAsync(args);

            var json = new StreamReader(result.StdOut).ReadToEnd();
            Assert.Equal("{\"foo\":\"bar\",\"lorem\":\"ipsum\"}", json);
        }

        [Fact]
        public async Task RedirectTest_ShouldShow3XXResponse_GivenStopAutoRedirects()
        {
            var args = new[]
            {
                "get", "http://localhost:5000/Redirect", "--stop-auto-redirects"
            };

            var result = await Https.ExecuteAsync(args);

            Assert.Equal("HTTP/1.1 301 Moved Permanently", result.Status);
            Assert.Equal("http://localhost:5000/Mirror", result.Headers["Location"]);
        }
    }
}
