using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Https.Tests
{
    [Collection(nameof(WebHostFixture))]
    public class ContentTypeTests
    {
        readonly WebHostFixture _fixture;
        public ContentTypeTests(WebHostFixture fixture) =>
            _fixture = fixture;

        [Fact]
        public async Task MirrorTest_ShouldReflectFormUrlEncoded()
        {
            var args = new[]
            {
                "post", $"{_fixture.HttpUrl}/Mirror", "--form", "foo=bar", "lorem=ipsum"
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
                "post", $"{_fixture.HttpUrl}/Mirror", "--json", "foo=bar", "lorem=ipsum"
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
                "post", $"{_fixture.HttpUrl}/Mirror", "--xml=root", "foo=bar", "lorem=ipsum"
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
    }
}
