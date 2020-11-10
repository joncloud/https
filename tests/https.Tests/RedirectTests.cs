using System.Threading.Tasks;
using Xunit;

namespace Https.Tests
{
    [Collection(nameof(WebHostFixture))]
    public class RedirectTests
    {
        readonly WebHostFixture _fixture;
        public RedirectTests(WebHostFixture fixture) =>
            _fixture = fixture;

        [Fact]
        public async Task RedirectTest_ShouldShow3XXResponse_GivenStopAutoRedirects()
        {
            var args = new[]
            {
                "get", $"{_fixture.HttpUrl}/Redirect", "--stop-auto-redirects"
            };

            var result = await Https.ExecuteAsync(args);

            Assert.Equal("HTTP/1.1 301 Moved Permanently", result.Status);
            Assert.Equal($"{_fixture.HttpUrl}/Mirror", result.Headers["Location"]);
        }
    }
}
