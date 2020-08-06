using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Https
{
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
}
