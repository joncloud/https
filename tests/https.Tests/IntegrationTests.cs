using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xunit;

namespace Https.Tests
{
    public class IntegrationTests : IClassFixture<WebHostFixture>
    {
        readonly WebHostFixture _fixture;
        public IntegrationTests(WebHostFixture fixture) =>
            _fixture = fixture;

        [Fact]
        public async Task MirrorTest_ShouldReflectFormUrlEncoded()
        {
            var args = new[]
            {
                "post", $"{_fixture.Url}/Mirror", "--form", "foo=bar", "lorem=ipsum"
            };

            var result = await Https.ExecuteAsync(args);

            var actual = new StreamReader(result.StdOut).ReadToEnd();
            Assert.Equal("foo=bar&lorem=ipsum", actual);
        }

        [Fact]
        public async Task MirrorTest_ShouldReflectJson()
        {
            var args = new[]
            {
                "post", $"{_fixture.Url}/Mirror", "--json", "foo=bar", "lorem=ipsum"
            };

            var result = await Https.ExecuteAsync(args);

            var actual = new StreamReader(result.StdOut).ReadToEnd();
            Assert.Equal("{\"foo\":\"bar\",\"lorem\":\"ipsum\"}", actual);
        }

        [Fact]
        public async Task MirrorTest_ShouldReflectXml()
        {
            var args = new[]
            {
                "post", $"{_fixture.Url}/Mirror", "--xml=root", "foo=bar", "lorem=ipsum"
            };

            var result = await Https.ExecuteAsync(args);

            var expected = new XDocument(
                new XElement(
                    "root",
                    new XElement("foo", "bar"),
                    new XElement("lorem", "ipsum")
                )
            ).ToString();
            var actual = XDocument.Load(
                new StreamReader(result.StdOut)
            ).ToString();

            Assert.Equal(expected, actual);
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
