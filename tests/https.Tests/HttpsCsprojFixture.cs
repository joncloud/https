using System.Xml.Linq;

namespace Https.Tests
{
    public class HttpsCsprojFixture
    {
        public XDocument Document { get; }

        public HttpsCsprojFixture()
        {
            var path = "../../../../../src/https/https.csproj";

            Document = XDocument.Load(path);
        }
    }
}
