using System.IO;
using System.Threading.Tasks;

namespace Https.Tests
{
    public static class Https
    {
        public static async Task<HttpsResult> ExecuteAsync(params string[] args)
        {
            using var stdin = new MemoryStream();

            return await ExecuteAsync(stdin, args);
        }

        public static async Task<HttpsResult> ExecuteAsync(Stream stdin, params string[] args)
        {
            var stdout = new MemoryStream();
            var stderr = new MemoryStream();
            
            var exitCode = await new Program(() => stderr, () => stdin, () => stdout, false)
                .RunAsync(args);

            return new HttpsResult(exitCode, stdout, stderr);
        }
    }
}
