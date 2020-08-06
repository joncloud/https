using System;
using System.Collections.Generic;

namespace Https
{
    class Options
    {
        public ContentType RequestContentType { get; }
        public string XmlRootName { get; }
        public bool IgnoreCertificate { get; }
        public TimeSpan? Timeout { get; }
        public bool Version { get; }
        public bool Help { get; }
        public bool StopAutoRedirects { get; }

        public bool RequiresHandler => IgnoreCertificate || StopAutoRedirects;

        public Options(ContentType requestContentType, string xmlRootName, bool ignoreCertificate, TimeSpan? timeout, bool version, bool help, bool stopAutoRedirects)
        {
            RequestContentType = requestContentType;
            XmlRootName = xmlRootName;
            IgnoreCertificate = ignoreCertificate;
            Timeout = timeout;
            Version = version;
            Help = help;
            StopAutoRedirects = stopAutoRedirects;
        }

        public static IEnumerable<string> GetOptionHelp()
        {
            yield return "--form                Renders the content arguments as application/x-www-form-urlencoded";
            yield return "--help                Show command line help.";
            yield return "--ignore-certificate  Prevents server certificate validation.";
            yield return "--json                Renders the content arguments as application/json.";
            yield return "--timeout=<VALUE>     Sets the timeout of the request using System.TimeSpan.TryParse (https://docs.microsoft.com/en-us/dotnet/api/system.timespan.parse)";
            yield return "--version             Displays the application verison.";
            yield return "--xml=<ROOT_NAME>     Renders the content arguments as application/xml using the optional xml root name.";
            yield return "--stop-auto-redirects Prevents redirects from automatically being processed.";
        }

        static int GetArgValueIndex(string arg)
        {
            var equalsIndex = arg.IndexOf('=');
            var spaceIndex = arg.IndexOf(' ');
            var index = equalsIndex > -1 && spaceIndex > -1
                ? Math.Min(equalsIndex, spaceIndex)
                : Math.Max(equalsIndex, spaceIndex);

            return index == -1 ? index : index + 1;
        }

        public static Options Parse(IEnumerable<string> args)
        {
            var requestContentType = ContentType.Json;
            var xmlRootName = default(string);
            var ignoreCertificate = false;
            var timeout = default(TimeSpan?);
            var help = false;
            var version = false;
            var stopAutoRedirects = false;
            foreach (var arg in args)
            {
                if (arg.StartsWith("--json"))
                {
                    requestContentType = ContentType.Json;
                }
                else if (arg.StartsWith("--xml"))
                {
                    var index = GetArgValueIndex(arg);
                    if (index == -1)
                    {
                        xmlRootName = "xml";
                    }
                    else
                    {
                        xmlRootName = arg.Substring(index).Trim();
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
                else if (arg.StartsWith("--timeout"))
                {
                    var index = GetArgValueIndex(arg);
                    if (index > -1)
                    {
                        var s = arg.Substring(index).Trim();
                        if (TimeSpan.TryParse(s, out var to) && to > TimeSpan.Zero)
                        {
                            timeout = to;
                        }
                    }
                }
                else if (arg.StartsWith("--version"))
                {
                    version = true;
                }
                else if (arg.StartsWith("--help") || arg.StartsWith("-?") || arg.StartsWith("help"))
                {
                    help = true;
                }
                else if (arg.StartsWith("--stop-auto-redirects"))
                {
                    stopAutoRedirects = true;
                }
            }
            return new Options(requestContentType, xmlRootName, ignoreCertificate, timeout, version, help, stopAutoRedirects);
        }
    }
}
