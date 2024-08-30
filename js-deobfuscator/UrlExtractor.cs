using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class UrlExtractor
{
    private const string IMAGE_URL_KEY = "imageUrl";

    private readonly string _content;

    public UrlExtractor(string content)
    {
        _content = content;
    }

    public string ExtractRunPeUrl()
    {
        var pairs = Regex.Matches(_content, RegexPatterns.URL_PAIRS_PATTERN)
            .Cast<Match>()
            .Select(m => new KeyValuePair<string, string>(
                m.Groups[1].Value,
                m.Groups[2].Value.Trim()
            ))
            .ToList();

        return pairs.FirstOrDefault(p => p.Key == IMAGE_URL_KEY).Value.Trim('\'');
    }

    public string ExtractPayloadUrl()
    {
        Match m = Regex.Match(_content, RegexPatterns.PAYLOAD_URL_PATTERN);
        if (m.Success)
        {
            string revUrl = new string(m.Groups[1].Value.Reverse().ToArray());
            return revUrl.TrimStart('&');
        }
        m = Regex.Match(_content, RegexPatterns.PAYLOAD_URL_PATTERN3008);
        if (m.Success)
        {
          
            string revUrl = new string(m.Groups[1].Value.Reverse().ToArray());
            return revUrl.TrimStart('&');
        }
        return string.Empty;
    }
}