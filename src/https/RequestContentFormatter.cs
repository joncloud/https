using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
}
