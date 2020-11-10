using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Https.Tests
{
    [Collection(nameof(WebHostFixture))]
    public class CertificateTests
    {
        readonly WebHostFixture _fixture;
        public CertificateTests(WebHostFixture fixture) =>
            _fixture = fixture;

        [Fact]
        public async Task UntrustedCertificate_ShouldPresentErrorMessage_GivenCertificatesAreHonored()
        {
            var args = new[]
            {
                "post", $"{_fixture.HttpsUrl}/Mirror", "--form", "foo=bar", "lorem=ipsum"
            };

            var result = await Https.ExecuteAsync(args);

            Assert.Equal(1, result.ExitCode);
        }

        [Fact]
        public async Task UntrustedCertificate_ShouldProcess_GivenCertificatesAreIgnored()
        {
            var args = new[]
            {
                "post", $"{_fixture.HttpsUrl}/Mirror", "--form", "--ignore-certificate", "foo=bar", "lorem=ipsum"
            };

            var result = await Https.ExecuteAsync(args);

            var actual = new StreamReader(result.StdOut).ReadToEnd();
            Assert.Equal("foo=bar&lorem=ipsum", actual);
        }
    }
}
