namespace Https.Tests
{
    public class ReadmeFixture
    {
        public Readme Readme { get; }
        public ReadmeFixture()
        {
            Readme = new Readme(
                "../../../../../README.md"
            );
        }
    }
}
