using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Https
{
    class Renderer
    {
        readonly StreamWriter _output;
        readonly StreamWriter _info;
        public Renderer(StreamWriter output, StreamWriter info)
        {
            _output = output;
            _info = info;
        }

        public async Task WriteResponse(HttpResponseMessage response)
        {
            _info.Write("HTTP/");
            _info.Write(response.Version);
            _info.Write(" ");
            _info.Write((int)response.StatusCode);
            _info.Write(" ");
            _info.WriteLine(response.ReasonPhrase);

            WriteHeaders(response.Headers, response.Content.Headers);
            await ResponseContentFormatter.As(response, _output);
        }

        public void WriteHeaders(HttpResponseHeaders responseHeaders, HttpContentHeaders contentHeaders)
        {
            var headers = responseHeaders.Concat(contentHeaders);
            
            foreach (var header in headers)
            {
                foreach (var value in header.Value)
                {
                    _info.Write(header.Key);
                    _info.Write(":");
                    _info.Write(" ");
                    _info.WriteLine(value);
                }
            }
        }

        public void WriteException(Exception ex)
        {
            var help = WriteException(ex, 0);
            switch (help)
            {
                case ExceptionHelp.Timeout:
                    _info.WriteLine("Request failed to complete within timeout. Try increasing the timeout with the --timeout flag");
                    break;
                case ExceptionHelp.IgnoreCertificate:
                    _info.WriteLine("Ensure you trust the server certificate or try using the --ignore-certificate flag");
                    break;
            }
        }
        
        ExceptionHelp WriteException(Exception ex, int depth)
        {
            if (depth > 0)
            {
                _info.Write(new string('\t', depth));
            }
            _info.WriteLine(ex.Message);

            var exceptionHelp = ExceptionHelp.None;
            if (ex is TaskCanceledException || ex is OperationCanceledException)
            {
                return ExceptionHelp.Timeout;
            }
            else
            {
                switch (ex.Message)
                {
                    case "The SSL connection could not be established, see inner exception.":
                        exceptionHelp = ExceptionHelp.IgnoreCertificate;
                        break;
                }
            }

            if (ex.InnerException != null)
            {
                var otherHelp = WriteException(ex.InnerException, depth + 1);
                if (otherHelp != ExceptionHelp.None)
                {
                    return otherHelp;
                }
            }

            return exceptionHelp;
        }

        enum ExceptionHelp
        {
            None = 0,
            IgnoreCertificate = 1,
            Timeout = 2
        }
    }
}
