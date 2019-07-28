using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Https.Tests
{
    public class HttpsResult : IDisposable
    {
        public int ExitCode { get; }
        public MemoryStream StdOut { get; }
        public string Status { get; }
        public IReadOnlyDictionary<string, string> Headers { get; }

        public HttpsResult(int exitCode, MemoryStream stdout, MemoryStream stderr)
        {
            ExitCode = exitCode;

            StdOut = stdout;
            StdOut.Position = 0;

            stderr.Position = 0;
            var lines = new StreamReader(stderr)
                .ReadToEnd()
                .Split(Environment.NewLine);

            Status = lines[0];

            var headers = new Dictionary<string, string>();
            foreach (var line in lines.Skip(1))
            {
                var pos = line.IndexOf(':');
                if (pos > -1)
                {
                    var key = line.Substring(0, pos);
                    var value = line.Substring(pos + 1);
                    headers[key] = value;
                }
            }
            Headers = headers;

            stderr.Dispose();
        }

        public void Dispose()
        {
            StdOut.Dispose();
        }
    }
}
