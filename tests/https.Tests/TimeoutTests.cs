using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit;

namespace Https.Tests
{
    [Collection(nameof(WebHostFixture))]
    public class TimeoutTests
    {
        readonly WebHostFixture _fixture;
        public TimeoutTests(WebHostFixture fixture) =>
            _fixture = fixture;

        [Fact]
        public async Task TimeoutTest_ShouldRespectTimeoutOption()
        {
            var delay = 1_000;
            var args = new[]
            {
                "post", $"{_fixture.HttpUrl}/Timeout?delay={delay * 4}", $"--timeout={TimeSpan.FromMilliseconds(delay)}"
            };

            var result = await Https.ExecuteAsync(args);

            Assert.Equal(1, result.ExitCode);

            var stdout = new StreamReader(result.StdOut).ReadToEnd();
            var expectedOut = "";
            Assert.Equal(expectedOut, stdout);

            var stderr = new StreamReader(result.StdErr).ReadToEnd();
            var expectedErr = string.Join(Environment.NewLine, new[]
            {
                "The request was canceled due to the configured HttpClient.Timeout of 1 seconds elapsing.",
                "Request failed to complete within timeout. Try increasing the timeout with the --timeout flag",
                ""
            });
            Assert.Equal(expectedErr, stderr);
        }
    }
}
