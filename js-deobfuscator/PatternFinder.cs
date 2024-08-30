using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System;

public class PatternFinder
{
    private const string NO_PATTERN_MSG = "No obfuscation pattern found.";
    private const string PATTERN_FOUND_MSG = "Found potential obfuscation pattern with variable: {0}";
    private const string VAR_NOT_FOUND_MSG = "Could not find the value for '{0}'. Skipping replacement.";

    public const string URL_FOUND = "{0} URL found: {1}";
    public const string URL_NOT_FOUND = "{0} URL not found in the configuration.";
    public const string SAVE_PROMPT = "Enter the path to save the {0} file (or press Enter to skip): ";
    public const string DOWNLOAD_SKIPPED = "{0} download skipped.";
    public const string DOWNLOAD_FAILED = "{0} download failed.";
    public const string PROCESS_FAILED = "{0} processing failed.";

    private string _script;

    public PatternFinder(string script)
    {
        _script = script;
    }

    public string FindAndReplace()
    {
        var matches = Regex.Matches(_script, RegexPatterns.SPLIT_JOIN_PATTERN);

        if (matches.Count == 0)
        {
            ConsoleHelper.WriteLine(NO_PATTERN_MSG, ConsoleColor.Yellow);
            return _script;
        }

        foreach (Match m in matches)
        {
            string splitVar = m.Value;
            ConsoleHelper.WriteLine(string.Format(PATTERN_FOUND_MSG, splitVar), ConsoleColor.Cyan);

            var varMatch = Regex.Match(_script, string.Format(RegexPatterns.VARIABLE_PATTERN, splitVar));

            if (varMatch.Success)
            {
                string splitVal = varMatch.Groups[1].Value;
                _script = _script.Replace(splitVal, "");
            }
            else
            {
                ConsoleHelper.WriteLine(string.Format(VAR_NOT_FOUND_MSG, splitVar), ConsoleColor.Yellow);
            }
        }

        return _script;
    }

    public string Process(string cleaned)
    {
        _script = cleaned;

        var match = Regex.Match(_script, RegexPatterns.PLUS_PATTERN);

        if (!match.Success)
        {
            ConsoleHelper.WriteLine(NO_PATTERN_MSG, ConsoleColor.Yellow);
            return _script;
        }

        string foundPat = match.Value.Trim();
        ConsoleHelper.WriteLine($"Found pattern: '{foundPat}'", ConsoleColor.Cyan);

        var valMatch = Regex.Match(_script, string.Format(RegexPatterns.VARIABLE_PATTERN, foundPat));

        if (!valMatch.Success)
        {
            ConsoleHelper.WriteLine($"Could not find the value for '{foundPat}'", ConsoleColor.Yellow);
            return _script;
        }

        string val = valMatch.Groups[1].Value;

        var match2 = Regex.Match(_script, RegexPatterns.A_PATTERN);

        if (!match2.Success)
        {
            ConsoleHelper.WriteLine(NO_PATTERN_MSG, ConsoleColor.Yellow);
            return _script;
        }

        ConsoleHelper.WriteLine($"Found pattern: '{match2.Value.Trim()}'", ConsoleColor.Cyan);

        string matchedVal = match2.Groups[1].Value;
        string revMatchedVal = new string(matchedVal.Reverse().ToArray());
        _script = _script.Replace(revMatchedVal, "A");

        var splitVarMatch = Regex.Match(_script, string.Format(RegexPatterns.VARIABLE_PATTERN, foundPat));
        string splitVal = splitVarMatch.Groups[1].Value;
        string revSplitVal = new string(splitVal.Reverse().ToArray());

        byte[] decodedBytes = Convert.FromBase64String(revSplitVal);
        string decodedStr = Encoding.UTF8.GetString(decodedBytes);
        string cleanedStr = new string(decodedStr.Where(c => !char.IsControl(c) && (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c))).ToArray());

        _script = _script.Replace(splitVal, cleanedStr);
        _script = Regex.Replace(_script, RegexPatterns.LONG_RANDOM_STRING_PATTERN, "");

        _script = Regex.Replace(_script, RegexPatterns.EMPTY_LINES_PATTERN, "", RegexOptions.Multiline);


        ProcessUrls(cleanedStr);

        return _script;
    }

    private void ProcessUrls(string cleanedStr)
    {
        var extractor = new UrlExtractor(cleanedStr);
        var downloader = new FileDownloader();

        ProcessUrl("RunPE/Loader", extractor.ExtractRunPeUrl(), downloader.ProcessRunPE);
        ProcessUrl("Payload", extractor.ExtractPayloadUrl(), (url, path) => downloader.Download(url, path, "Payload"));
    }

    private void ProcessUrl(string type, string url, Func<string, string, bool> downloadFunc)
    {
        if (string.IsNullOrEmpty(url))
        {
            ConsoleHelper.WriteLine(string.Format(URL_NOT_FOUND, type), ConsoleColor.Yellow);
            return;
        }

        ConsoleHelper.WriteLine(string.Format(URL_FOUND, type, url), ConsoleColor.Cyan);
        string path = ConsoleHelper.PromptForInput(string.Format(SAVE_PROMPT, type));

        if (string.IsNullOrWhiteSpace(path))
        {
            ConsoleHelper.WriteLine(string.Format(DOWNLOAD_SKIPPED, type), ConsoleColor.Yellow);
            return;
        }

        bool success = downloadFunc(url, path);
        if (!success)
        {
            ConsoleHelper.WriteLine(string.Format(type.Contains("RunPE") ? PROCESS_FAILED : DOWNLOAD_FAILED, type), ConsoleColor.Yellow);
        }
    }
}