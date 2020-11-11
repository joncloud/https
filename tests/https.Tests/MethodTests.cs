using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Https.Tests
{
    [Collection(nameof(WebHostFixture))]
    public class MethodTests
    {
        readonly WebHostFixture _fixture;
        public MethodTests(WebHostFixture fixture) =>
            _fixture = fixture;

        [Fact]
        public async Task MethodTest_ShouldHandleHead()
        {
            var args = new[]
            {
                "head", $"{_fixture.HttpUrl}/Mirror"
            };

            var result = await Https.ExecuteAsync(args);

            var actual = new StreamReader(result.StdOut).ReadToEnd();
            Assert.Equal("", actual);
            Assert.Equal(0, result.ExitCode);
        }

        // Most of these methods probably should be tested differently,
        // but apparently they work through HttpClient and ASP.NET Core.
        [InlineData(nameof(HttpMethod.Delete))]
        [InlineData(nameof(HttpMethod.Get))]
        [InlineData(nameof(HttpMethod.Options))]
        [InlineData(nameof(HttpMethod.Patch))]
        [InlineData(nameof(HttpMethod.Post))]
        [InlineData(nameof(HttpMethod.Put))]
        [InlineData(nameof(HttpMethod.Trace))]
        [Theory]
        public async Task MethodTest_ShouldHandleMethod(string method)
        {
            var args = new[]
            {
                method, $"{_fixture.HttpUrl}/Mirror", "--form", "foo=bar", "lorem=ipsum"
            };

            var result = await Https.ExecuteAsync(args);

            var actual = new StreamReader(result.StdOut).ReadToEnd();
            Assert.Equal("foo=bar&lorem=ipsum", actual);
        }
    }
}
