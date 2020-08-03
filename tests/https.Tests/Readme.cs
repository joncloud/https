using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Https.Tests
{
    public class Readme
    {
        public string InstallationVersion { get; }
        public string UsageDocumentation { get; }

        public Readme(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

            var sb = new StringBuilder();
            InstallationVersion = "";
            UsageDocumentation = "";

            var lines = File.ReadLines(path);
            var record = false;
            foreach (var line in lines)
            {
               
                if (line.StartsWith("```bash"))
                {
                    record = true;
                }
                else if (line.StartsWith("```"))
                {
                    var text = sb.ToString();
                    sb.Clear();

                    if (text.StartsWith("dotnet tool"))
                    {
                        var match = Regex.Match(text, "dotnet tool install --global https --version (.+)-\\*");
                        if (match.Success)
                        {
                            InstallationVersion = match.Groups[1].Value;
                        }
                    }
                    else if (text.StartsWith("Usage"))
                    {
                        UsageDocumentation = text;
                    }

                    record = false;
                }
                else if (record)
                {
                    sb.AppendLine(line);
                }
            }
        }
    }
}
