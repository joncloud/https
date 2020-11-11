using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Https.Tests
{
    public class ReadmeTests : IClassFixture<HttpsCsprojFixture>, IClassFixture<ReadmeFixture>
    {
        readonly HttpsCsprojFixture _httpsCsprojFixture;
        readonly ReadmeFixture _readmeFixture;
        public ReadmeTests(HttpsCsprojFixture httpsCsprojFixture, ReadmeFixture readmeFixture)
        {
            _httpsCsprojFixture = httpsCsprojFixture;
            _readmeFixture = readmeFixture;
        }

        [Fact]
        public void Installation_ShouldListSameVersionAsCsproj()
        {
            var versionPrefixElement = _httpsCsprojFixture.VersionPrefix;

            var expected = versionPrefixElement.Value;
            var actual = _readmeFixture.Readme.InstallationVersion;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task Usage_ShouldListHelpDocument()
        {
            var httpsResult = await Https.ExecuteAsync("--help");

            using var reader = new StreamReader(httpsResult.StdOut);

            var expected = reader.ReadToEnd().TrimEnd();
            var actual = _readmeFixture.Readme.UsageDocumentation.TrimEnd();

            Assert.Equal(expected, actual);
        }
    }
}
