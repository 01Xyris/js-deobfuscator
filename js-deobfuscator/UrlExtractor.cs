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
        string revUrl = TryExtractUrl(RegexPatterns.PAYLOAD_URL_PATTERN);

        if (revUrl == null)
        {
            revUrl = TryExtractUrl(RegexPatterns.PAYLOAD_URL_PATTERN3008);
        }

        if (revUrl != null)
        {
      
            return revUrl.TrimStart('&');
        }

        ConsoleHelper.WriteLine("No valid URL found in content.", System.ConsoleColor.Red);
        return string.Empty; 
    }

    private string TryExtractUrl(string pattern)
    {
        Match match = Regex.Match(_content, pattern);
        if (match.Success)
        {
            string reversedUrl = new string(match.Value.Reverse().ToArray());
            return reversedUrl;
        }

        return null;
    }

}