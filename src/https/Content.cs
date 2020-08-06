namespace Https
{
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
            var equalsIndex = s.IndexOf('=');
            var colonIndex = s.IndexOf(':');
            if (equalsIndex == -1 && colonIndex == -1)
            {
                content = default;
                return false;
            }

            var contentType = default(ContentLocation);
            var index = default(int);
            if (equalsIndex > -1 && colonIndex > -1)
            {
                if (equalsIndex < colonIndex)
                {
                    contentType = ContentLocation.Body;
                    index = equalsIndex;
                }
                else
                {
                    contentType = ContentLocation.Header;
                    index = colonIndex;
                }
            }
            else if (equalsIndex > -1)
            {
                contentType = ContentLocation.Body;
                index = equalsIndex;
            }
            else
            {
                contentType = ContentLocation.Header;
                index = colonIndex;
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
}
