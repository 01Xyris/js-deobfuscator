using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.Generic;

public class PatternFinder
{
    private string _script;
    private Dictionary<string, string> _renameMap = new Dictionary<string, string>();
    private int _varCounter = 0;
    private int _funcCounter = 0;
    private const string NO_PATTERN = "No obfuscation pattern found.";
    private const string PATTERN_FOUND = "Found potential obfuscation pattern with variable: {0}";
    private const string VAR_NOT_FOUND = "Could not find the value for '{0}'. Skipping replacement.";
    public const string URL_FOUND = "{0} URL found: {1}";
    public const string URL_NOT_FOUND = "{0} URL not found in the configuration.";
    public const string SAVE_PROMPT = "Enter the path to save the {0} file (or press Enter to skip): ";
    public const string SKIP_MSG = "{0} download skipped.";
    public const string FAIL_MSG = "{0} {1} failed.";

    public static string value = "";

    public PatternFinder(string script) => _script = script;
    private void RenameLongIdentifiers()
    {
        string pattern = @"\b(var\s+)?([a-zA-Z_$][a-zA-Z0-9_$]{8,})\b(?!\s*\()|\b(function\s+)?([a-zA-Z_$][a-zA-Z0-9_$]{8,})\s*\(";

        StringBuilder result = new StringBuilder();
        int lastIndex = 0;
        bool inString = false;
        char stringChar = '\0';

        foreach (Match match in Regex.Matches(_script, @"("".*?""|'.*?'|[^""']+)"))
        {
            string part = match.Value;
            if (part.StartsWith("\"") || part.StartsWith("'"))
            {
                result.Append(part);
            }
            else
            {
                string renamed = Regex.Replace(part, pattern, m =>
                {
                    string fullMatch = m.Value;
                    string identifier = m.Groups[2].Success ? m.Groups[2].Value : m.Groups[4].Value;
                    bool isFunction = m.Groups[4].Success;

                    if (!_renameMap.ContainsKey(identifier))
                    {
                        string newName = isFunction ? $"func_{_funcCounter++}" : $"var_{_varCounter++}";
                        _renameMap[identifier] = newName;
                    }

                    return fullMatch.Replace(identifier, _renameMap[identifier]);
                });
                result.Append(renamed);
            }
        }

        _script = result.ToString();
    }
    

public string FindReplace()
    {
        var matches = Regex.Matches(_script, RegexPatterns.SPLIT_JOIN_PATTERN);
        if (matches.Count == 0)
        {
            ConsoleHelper.WriteLine(NO_PATTERN, ConsoleColor.Yellow);
            matches = Regex.Matches(_script, RegexPatterns.VBS_JOIN);
        }

        foreach (Match m in matches)
        {
            string splitVar = m.Value;
            ConsoleHelper.WriteLine(string.Format(PATTERN_FOUND, splitVar), ConsoleColor.Cyan);
            var varMatch = Regex.Match(_script, string.Format(RegexPatterns.VARIABLE_PATTERN, splitVar));
            if (varMatch.Success) ProcessJS(varMatch);
            else ProcessVBS(splitVar);
        }
        return _script;
    }

    public string Process(string cleaned)
    {
        _script = cleaned;

        if (!string.IsNullOrEmpty(value))
            _script = _script.Replace(new string(value.Reverse().ToArray()), "");

        foreach (Match m in Regex.Matches(_script, RegexPatterns.PLUS_PATTERN))
            ProcessJSPlus(m);

        foreach (Match m in Regex.Matches(_script, RegexPatterns.VBS_JOIN))
            ProcessVBS(m.Value);

        CleanUp();

        RenameLongIdentifiers();
        Console.WriteLine("Result after processing:");
        return _script;
    }

    private void ProcessJS(Match varMatch)
    {
        value = varMatch.Groups[1].Value;
        _script = _script.Replace(value, "");
        string revValue = new string(value.Reverse().ToArray());
        _script = _script.Replace(revValue.Split(';')[0], "A");
    }

    private void ProcessVBS(string splitVar)
    {
        var varMatch = Regex.Match(_script, string.Format(RegexPatterns.VBS_VARIABLE_PATTERN, splitVar));
        if (varMatch.Success)
        {
            value = varMatch.Groups[1].Value;
            _script = _script.Replace(value, "");
            string revValue = new string(value.Reverse().ToArray());
            _script = _script.Replace(revValue.Split(';')[0], "A");

            foreach (Match d in Regex.Matches(_script, RegexPatterns.VBS_VARIABLE_PATTERN2))
                ProcessVBSExtra(d, value);
        }
    }

    private void ProcessJSPlus(Match match)
    {
        string foundPat = match.Value.Trim();
        ConsoleHelper.WriteLine($"Found pattern: '{foundPat}'", ConsoleColor.Cyan);
        var splitMatch = Regex.Match(_script, string.Format(RegexPatterns.VARIABLE_PATTERN, foundPat));
        if (splitMatch.Success)
        {
            string splitVal = splitMatch.Groups[1].Value;
            string revSplitVal = new string(splitVal.Reverse().ToArray());
            try
            {
                byte[] decodedBytes = Convert.FromBase64String(revSplitVal);
                string decodedStr = Encoding.UTF8.GetString(decodedBytes);
                string cleanedStr = new string(decodedStr.Where(c => !char.IsControl(c) && (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c))).ToArray());
                ProcessUrls(cleanedStr);
                _script = _script.Replace(splitVal, cleanedStr);
            }
            catch (FormatException ex)
            {
                ConsoleHelper.WriteLine($"Base64 decode error: {ex.Message}", ConsoleColor.Red);
            }
        }
    }

    private void ProcessVBSExtra(Match match, string splitVal)
    {
        string yesRev = new string(match.Value.Reverse().ToArray());
        try
        {
            byte[] decodedBytes = Convert.FromBase64String(yesRev);
            string decodedStr = Encoding.UTF8.GetString(decodedBytes);
            string cleanedStr = new string(decodedStr.Where(c => !char.IsControl(c) && (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsWhiteSpace(c))).ToArray());
            ProcessUrls(cleanedStr);
            _script = _script.Replace(splitVal, cleanedStr);
        }
        catch (FormatException ex)
        {
            ConsoleHelper.WriteLine($"Base64 decode error: {ex.Message}", ConsoleColor.Red);
        }
    }

    private void CleanUp()
    {
        _script = Regex.Replace(_script, RegexPatterns.LONG_RANDOM_STRING_PATTERN, "", RegexOptions.Multiline);
        _script = Regex.Replace(_script, RegexPatterns.EMPTY_LINES_PATTERN, "", RegexOptions.Multiline);
    }

    private void ProcessUrls(string cleanedStr)
    {
        var extractor = new UrlExtractor(cleanedStr);
        var downloader = new FileDownloader();
        ProcessUrl("RunPE/Loader", extractor.ExtractRunPeUrl(), downloader.ProcessRunPE);
        ProcessUrl("Payload", extractor.ExtractPayloadUrl(), (url, path) => downloader.Download(url, path));
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
            ConsoleHelper.WriteLine(string.Format(SKIP_MSG, type), ConsoleColor.Yellow);
            return;
        }
        bool success = downloadFunc(url, path);
        if (!success)
            ConsoleHelper.WriteLine(string.Format(FAIL_MSG, type, type.Contains("RunPE") ? "processing" : "download"), ConsoleColor.Yellow);
    }
}