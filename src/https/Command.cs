using System;
using System.Net.Http;

namespace Https
{
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
            if (s.StartsWith('-') || s == "help")
            {
                command = default;
                return false;
            }

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
