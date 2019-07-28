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
        }
    }
}
