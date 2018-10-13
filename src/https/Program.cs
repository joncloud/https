using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml.Linq;
using Utf8Json;

namespace Https
{
    static class RequestContentFormatter
    {
        public static HttpContent As(ContentType requestContentType, IEnumerable<Content> contents, string xmlRootName)
        {
            switch (requestContentType)
            {
                case ContentType.FormUrlEncoded:
                    return AsFormUrlEncoded(contents);
                case ContentType.Xml:
                    return AsXml(xmlRootName, contents);
                case ContentType.Json:
                    return AsJson(contents);
                default:
                    throw new ArgumentOutOfRangeException(nameof(requestContentType), requestContentType, "Invalid request content type");
            }
        }

        public static HttpContent AsFormUrlEncoded(IEnumerable<Content> contents)
        {
            var pairs = contents.Select(content => new KeyValuePair<string, string>(content.Property, content.Value));
            return new FormUrlEncodedContent(pairs);
        }

        public static HttpContent AsXml(string root, IEnumerable<Content> contents)
        {
            var xdocument = new XDocument(
                new XElement(
                    root,
                    contents.Select(content => new XElement(content.Property, content.Value))
                )
            );

            var stream = new MemoryStream();
            xdocument.Save(stream);
            stream.Position = 0;
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
            return streamContent;
        }

        public static HttpContent AsJson(IEnumerable<Content> contents)
        {
            var bytes = ArrayPool<byte>.Shared.Rent(100);
            var writer = new JsonWriter(bytes);

            writer.WriteBeginObject();
            var counter = 0;
            foreach (var content in contents)
            {
                if (counter++ > 0)
                {
                    writer.WriteValueSeparator();
                }

                writer.WritePropertyName(content.Property);

                writer.WriteString(content.Value);
            }
            writer.WriteEndObject();

            var stream = new MemoryStream(writer.ToUtf8ByteArray());
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/json")
            {
                CharSet = "utf-8"
            };
            return streamContent;
        }
    }

    static class ResponseContentFormatter
    {
        public static async Task As(HttpResponseMessage response, StreamWriter target)
        {
            using (var stream = await response.Content.ReadAsStreamAsync())
            {
                switch (response.Content.Headers.ContentType?.MediaType)
                {
                    case "application/json":
                        await AsJson(stream, target);
                        break;
                    default:
                        await AsOrigin(stream, target);
                        break;
                    case "application/xml":
                        await AsXml(stream, target);
                        break;
                }
            }
        }

        static async Task AsOrigin(Stream source, StreamWriter target)
        {
            await source.CopyToAsync(target.BaseStream);
        }

        static Task AsXml(Stream source, StreamWriter target) =>
            AsOrigin(source, target);

        static Task AsJson(Stream source, StreamWriter target) =>
            AsOrigin(source, target);
    }

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

        void Help()
        {
            var stream = _stdout();
            var writer = new StreamWriter(stream) { AutoFlush = true };
            writer.Write("dotnet-https ");
            writer.WriteLine(typeof(Program).Assembly.GetName().Version);
            writer.WriteLine("https [method] [uri] [options] [content]");
            writer.WriteLine("For example https put httpbin.org/put hello=world");
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
                        Help();
                        return 1;
                    }
                }
            }
            else if (!Command.TryParse(args[0], out command))
            {
                Help();
                return 1;
            }

            var optionArgs = args.Skip(2).TakeWhile(x => x.Length > 0 && x[0] == '-');
            var options = Options.Parse(optionArgs);

            var contentArgs = args.Skip(2).SkipWhile(x => x.Length > 0 && x[0] == '-');

            var stderr = _stderr();
            var stderrWriter = new StreamWriter(stderr) { AutoFlush = true };
            var stdout = _stdout();
            var stdoutWriter = new StreamWriter(stdout) { AutoFlush = true };
            {
                var renderer = new Renderer(stdoutWriter, stderrWriter);
                
                var http = options.IgnoreCertificate
                    ? new HttpClient(
                        new HttpClientHandler
                        {
                            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                        }
                    )
                    : new HttpClient();

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
    }

    enum ContentType
    {
        Json = 1,
        FormUrlEncoded = 2,
        Xml = 3
    }

    class Options
    {
        public ContentType RequestContentType { get; }
        public string XmlRootName { get; }
        public bool IgnoreCertificate { get; }

        public Options(ContentType requestContentType, string xmlRootName, bool ignoreCertificate)
        {
            RequestContentType = requestContentType;
            XmlRootName = xmlRootName;
            IgnoreCertificate = ignoreCertificate;
        }
        public static Options Parse(IEnumerable<string> args)
        {
            var requestContentType = ContentType.Json;
            var xmlRootName = default(string);
            var ignoreCertificate = false;
            foreach (var arg in args)
            {
                if (arg.StartsWith("--json"))
                {
                    requestContentType = ContentType.Json;
                }
                else if (arg.StartsWith("--xml"))
                {
                    var index = arg.IndexOf('=');
                    if (index == -1)
                    {
                        xmlRootName = "xml";
                    }
                    else
                    {
                        xmlRootName = arg.Substring(index + 1).Trim();
                        if (string.IsNullOrEmpty(xmlRootName))
                        {
                            xmlRootName = "xml";
                        }
                    }
                    requestContentType = ContentType.Xml;
                }
                else if (arg.StartsWith("--form"))
                {
                    requestContentType = ContentType.FormUrlEncoded;
                }
                else if (arg.StartsWith("--ignore-certificate"))
                {
                    ignoreCertificate = true;
                }
            }
            return new Options(requestContentType, xmlRootName, ignoreCertificate);
        }
    }

    enum ContentLocation
    {
        Body = 1,
        Header = 2
    }

    class Content
    {
        public ContentLocation ContentLocation { get; }
        public string Property { get; }
        public string Value { get; }
        Content(ContentLocation contentLocation, string property, string value)
        {
            ContentLocation = contentLocation;
            Property = property;
            Value = value;
        }

        public static bool TryParse(string s, out Content content)
        {
            var index = s.IndexOf('=');
            var contentType = default(ContentLocation);
            if (index == -1)
            {
                index = s.IndexOf(':');
                if (index == -1)
                {
                    content = default;
                    return false;
                }
                else
                {
                    contentType = ContentLocation.Header;
                }
            }
            else
            {
                contentType = ContentLocation.Body;
            }

            var property = s.Substring(0, index);
            if (property.Length == 0)
            {
                content = default;
                return false;
            }

            var value = s.Substring(index + 1);
            content = new Content(contentType, property, value);
            return true;
        }
    }

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
            switch (ex.Message)
            {
                case "The SSL connection could not be established, see inner exception.":
                    exceptionHelp = ExceptionHelp.IgnoreCertificate;
                    break;
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
        }
    }


    struct Command
    {
        public HttpMethod Method { get; }
        public Uri Uri { get; }

        Command(Uri uri)
        {
            Method = default;
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }
        Command(HttpMethod method, Uri uri)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }

        static bool StartsWithHttp(string s) =>
            s.Length > 6 && s[0] == 'h' && s[1] == 't' && s[2] == 't' && s[3] == 'p' && (s[4] == ':' || (s[4] == 's' && s[5] == ':'));

        static bool TryParseUri(string s, out Uri uri)
        {
            if (!StartsWithHttp(s))
            {
                s = "https://" + s;
            }

            return Uri.TryCreate(s, UriKind.Absolute, out uri);
        }

        static bool TryParseMethod(string s, out HttpMethod method)
        {
            if (s.Equals(nameof(HttpMethod.Delete), StringComparison.OrdinalIgnoreCase))
            {
                method = HttpMethod.Delete;
                return true;
            }
            else if (s.Equals(nameof(HttpMethod.Get), StringComparison.OrdinalIgnoreCase))
            {
                method = HttpMethod.Get;
                return true;
            }
            else if (s.Equals(nameof(HttpMethod.Head), StringComparison.OrdinalIgnoreCase))
            {
                method = HttpMethod.Head;
                return true;
            }
            else if (s.Equals(nameof(HttpMethod.Options), StringComparison.OrdinalIgnoreCase))
            {
                method = HttpMethod.Options;
                return true;
            }
            else if (s.Equals(nameof(HttpMethod.Patch), StringComparison.OrdinalIgnoreCase))
            {
                method = HttpMethod.Patch;
                return true;
            }
            else if (s.Equals(nameof(HttpMethod.Post), StringComparison.OrdinalIgnoreCase))
            {
                method = HttpMethod.Post;
                return true;
            }
            else if (s.Equals(nameof(HttpMethod.Put), StringComparison.OrdinalIgnoreCase))
            {
                method = HttpMethod.Put;
                return true;
            }
            else if (s.Equals(nameof(HttpMethod.Trace), StringComparison.OrdinalIgnoreCase))
            {
                method = HttpMethod.Trace;
                return true;
            }

            method = default;
            return false;
        }

        public static bool TryParse(string methodText, string uriText, out Command command)
        {
            if (TryParseMethod(methodText, out var method) && TryParseUri(uriText, out var uri))
            {
                command = new Command(method, uri);
                return true;
            }
            else
            {
                command = default;
                return false;
            }
        }

        public static bool TryParse(string s, out Command command)
        {
            s = s.Trim();

            var index = s.IndexOf(' ');
            if (index == -1)
            {
                if (TryParseUri(s, out var uri))
                {
                    command = new Command(uri);
                    return true;
                }
                else
                {
                    command = default;
                    return false;
                }
            }
            else
            {
                var methodText = s.Substring(0, index);
                var uriText = s.Substring(index + 1);

                return TryParse(methodText, uriText, out command);
            }
        }
    }
}
