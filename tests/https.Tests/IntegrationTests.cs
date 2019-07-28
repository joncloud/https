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
                "post", _fixture.Url, "--json", "foo=bar", "lorem=ipsum"
            };

            var result = await Https.ExecuteAsync(args);

            var json = new StreamReader(result.StdOut).ReadToEnd();
            Assert.Equal("{\"foo\":\"bar\",\"lorem\":\"ipsum\"}", json);
        }
    }
}
