using System.IO;
using System;

public class Program
{
    private const string WELCOME_MSG = "JS-Deobfuscator by Xyris";
    private const string INVALID_PATH_MSG = "Please provide a valid file path as a command-line argument.";

    static void Main(string[] args)
    {
        ConsoleHelper.WriteLine(WELCOME_MSG, ConsoleColor.Cyan);

        if (args.Length == 0 || !File.Exists(args[0]))
        {
            ConsoleHelper.WriteLine(INVALID_PATH_MSG, ConsoleColor.Red);
            return;
        }

        string script = File.ReadAllText(args[0]);

        var finder = new PatternFinder(script);
        string cleaned = finder.FindAndReplace();
        string processed = finder.Process(cleaned);

        string outPath = Path.Combine(Path.GetDirectoryName(args[0]),
            Path.GetFileNameWithoutExtension(args[0]) + "_cleaned" + Path.GetExtension(args[0]));

        File.WriteAllText(outPath, processed);

        ConsoleHelper.WriteLine($"Cleaned script saved to: {outPath}", ConsoleColor.Green);
    }
}