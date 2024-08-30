using System;
using System.IO;
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
    public bool Download(string url, string savePath, string fileType)
    {
        using (WebClient client = new WebClient())
        {
            client.DownloadProgressChanged += (s, e) => 
            {
                ConsoleHelper.WriteLine($"{e.ProgressPercentage}%", ConsoleColor.Green, false);
      
            };

            client.DownloadFileCompleted += (s, e) => 
            {
                if (e.Error == null)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    ConsoleHelper.WriteLine(string.Format(DOWNLOAD_SUCCESS_MSG, fileType), ConsoleColor.Green);
                }
                else
                {
                    ConsoleHelper.WriteLine(string.Format(DOWNLOAD_FAILED_HOST_MSG, fileType), ConsoleColor.Red);
                }
        
            };

            try
            {
                string fullPath = Path.Combine(savePath, Path.GetFileName(new Uri(url).LocalPath));
                client.DownloadFileAsync(new Uri(url), fullPath);
                while (client.IsBusy) System.Threading.Thread.Sleep(100);
                return true;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                ConsoleHelper.WriteLine(string.Format(DOWNLOAD_FAILED_HOST_MSG, fileType), ConsoleColor.Red);
                Console.ResetColor();
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