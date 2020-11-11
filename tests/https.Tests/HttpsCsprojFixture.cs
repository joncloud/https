using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace Https.Tests
{
    public class HttpsCsprojFixture
    {
        public XDocument Document { get; }

        public XElement VersionPrefix 
        {
            get
            {
                var value = Document
                    .Root
                    .Elements("PropertyGroup")
                    .Elements("VersionPrefix")
                    .FirstOrDefault();

                Assert.NotNull(value);

                return value;
            }
        }

        public HttpsCsprojFixture()
        {
            var path = "../../../../../src/https/https.csproj";

            Document = XDocument.Load(path);
        }
    }
}
