using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Https.Tests
{
    public class VersionTests : IClassFixture<HttpsCsprojFixture>
    {
        readonly HttpsCsprojFixture _httpsCsprojFixture;
        public VersionTests(HttpsCsprojFixture httpsCsprojFixture)
        {
            _httpsCsprojFixture = httpsCsprojFixture;
        }

        [Fact]
        public async Task VersionFlag_ShouldReportVersion()
        {
            var args = new[] 
            {
                "--version"
            };

            var versionPrefix = _httpsCsprojFixture.VersionPrefix.Value;

            var expected = $"dotnet-https {versionPrefix}.0" + Environment.NewLine;
            var result = await Https.ExecuteAsync(args);
            var actual = new StreamReader(result.StdOut).ReadToEnd();

            Assert.Equal(expected, actual);
        }
    }
}