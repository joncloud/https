using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Https
{
    public class Program
    {
        static IEnumerable<Content> ParseContents(IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                if (Content.TryParse(arg, out var content))
                {
                    yield return content;
                }
            }
        }

        void Version()
        {
            var stream = _stdout();
            var writer = new StreamWriter(stream) { AutoFlush = true };
            writer.Write("dotnet-https ");
            writer.WriteLine(typeof(Program).Assembly.GetName().Version);
            writer.Flush();
        }

        void Help()
        {
            var stream = _stdout();
            var writer = new StreamWriter(stream) { AutoFlush = true };
            writer.WriteLine("Usage: https <METHOD> <URI> [options] [content]");
            writer.WriteLine("");
            writer.WriteLine("Submits HTTP requests. For example https put httpbin.org/put hello=world");
            writer.WriteLine("");
            writer.WriteLine("Arguments:");
            writer.WriteLine("  <METHOD>    HTTP method, i.e., get, head, post");
            writer.WriteLine("  <URI>       URI to send the request to. Leaving the protocol off the URI defaults to https://");
            writer.WriteLine("");
            writer.WriteLine("Options:");
            foreach (var option in Options.GetOptionHelp())
            {
                writer.Write("  ");
                writer.WriteLine(option);
            }
            writer.WriteLine("");
            writer.WriteLine("Content:");
            writer.WriteLine("Repeat as many content arguments to create content sent with the HTTP request. Alternatively pipe raw content send as the HTTP request content.");
            writer.WriteLine("  <KEY>=<VALUE>");
            writer.WriteLine("");
            writer.WriteLine("Headers:");
            writer.WriteLine("Repeat as many header arguments to assign headers for the HTTP request.");
            writer.WriteLine("  <KEY>:<VALUE>");
            writer.WriteLine("");

            writer.Flush();
        }

        static void AddHeaders(HttpRequestMessage request, IEnumerable<Content> contents)
        {
            foreach (var content in contents)
            {
                if (!request.Headers.TryAddWithoutValidation(content.Property, content.Value))
                {
                    if (!request.Content.Headers.TryAddWithoutValidation(content.Property, content.Value))
                    {
                        Console.Error.Write("Unexpected header: ");
                        Console.Error.WriteLine(content.Property);
                    }
                }
            }
        }

        readonly Func<Stream> _stderr;
        readonly Func<Stream> _stdin;
        readonly Func<Stream> _stdout;
        readonly bool _useStdin;
        public Program()
            : this(Console.OpenStandardError, Console.OpenStandardInput, Console.OpenStandardOutput, Console.IsInputRedirected)
        {

        }
        public Program(Func<Stream> stderr, Func<Stream> stdin, Func<Stream> stdout, bool useStdin)
        {
            _stderr = stderr;
            _stdin = stdin;
            _stdout = stdout;
            _useStdin = useStdin;
        }

        public static Task<int> Main(string[] args) =>
            new Program().RunAsync(args);

        int HandleOptionsOnly(string[] args)
        {
            var options = Options.Parse(args);
            if (options.Help)
            {
                Help();
                return 0;
            }
            else if(options.Version)
            {
                Version();
                return 0;
            }

            Help();
            return 1;
        }

        public async Task<int> RunAsync(string[] args)
        {
            if (!args.Any())
            {
                Help();
                return 1;
            }

            var command = default(Command);
            if (args.Length > 1)
            {
                if (!Command.TryParse(args[0], args[1], out command))
                {
                    if (!Command.TryParse(args[0], out command))
                    {
                        return HandleOptionsOnly(args);
                    }
                }
            }
            else if (!Command.TryParse(args[0], out command))
            {
                return HandleOptionsOnly(args);
            }

            var optionArgs = args.Skip(2).TakeWhile(x => x.Length > 0 && x[0] == '-');
            var options = Options.Parse(optionArgs);
            if (options.Help)
            {
                Help();
                return 0;
            }
            else if (options.Version)
            {
                Version();
                return 0;
            }

            var contentArgs = args.Skip(2).SkipWhile(x => x.Length > 0 && x[0] == '-');

            var stderr = _stderr();
            var stderrWriter = new StreamWriter(stderr) { AutoFlush = true };
            var stdout = _stdout();
            var stdoutWriter = new StreamWriter(stdout) { AutoFlush = true };
            {
                var renderer = new Renderer(stdoutWriter, stderrWriter);

                var http = CreateHttpClient(options);

                var request = new HttpRequestMessage(
                    command.Method ?? HttpMethod.Get,
                    command.Uri
                );

                if (_useStdin)
                {
                    var stream = _stdin();
                    request.Content = new StreamContent(stream);
                }

                var groups = ParseContents(contentArgs)
                    .GroupBy(content => content.ContentLocation)
                    .OrderBy(group => group.Key);
                foreach (var group in groups)
                {
                    switch (group.Key)
                    {
                        case ContentLocation.Body:
                            request.Content = RequestContentFormatter.As(options.RequestContentType, group, options.XmlRootName);
                            break;
                        case ContentLocation.Header:
                            AddHeaders(request, group);
                            break;
                    }
                }

                if (!request.Headers.UserAgent.Any())
                {
                    request.Headers.UserAgent.Add(
                        new ProductInfoHeaderValue(
                            "dotnet-https",
                            typeof(Program).Assembly.GetName().Version.ToString()
                        )
                    );
                }

                try
                {
                    var response = await http.SendAsync(request);

                    await renderer.WriteResponse(response);
                }
                catch (TaskCanceledException ex)
                {
                    renderer.WriteException(ex);
                    return 1;
                }
                catch (OperationCanceledException ex)
                {
                    renderer.WriteException(ex);
                    return 1;
                }
                catch (HttpRequestException ex)
                {
                    renderer.WriteException(ex);
                    return 1;
                }
            }

            stderrWriter.Flush();
            stdoutWriter.Flush();

            return 0;
        }

        static HttpClient CreateHttpClient(Options options)
        {
            var http = default(HttpClient);
            if (options.RequiresHandler)
            {
                var handler = new HttpClientHandler();
                if (options.IgnoreCertificate)
                {
                    handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
                }
                if (options.StopAutoRedirects)
                {
                    handler.AllowAutoRedirect = false;
                }
                http = new HttpClient(handler);
            }
            else
            {
                http = new HttpClient();
            }

            if (options.Timeout.HasValue)
            {
                http.Timeout = options.Timeout.Value;
            }

            return http;
        }
    }
}
