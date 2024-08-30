using System;

public static class ConsoleHelper
{
    public static void WriteLine(string message, ConsoleColor color, bool newLine = true)
    {
        Console.ForegroundColor = color;
        if (newLine)
            Console.WriteLine(message);
        else
            Console.Write(message);
        Console.ResetColor();
    }

    public static string PromptForInput(string prompt)
    {
        Console.Write(prompt);
        return Console.ReadLine();
    }
}