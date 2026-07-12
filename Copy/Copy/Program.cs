using System;
using System.Diagnostics;
using System.IO;

namespace Filecopy
{
    internal class Program
    {
        static void Main(string[] args)
        {
            CopyOptions options = ArgumentsParser.Parse(args);
            if (options == null) return;

            FileCopier copier = new FileCopier();
            copier.Copy(options);
        }
    }

    public class CopyOptions
    {
        public string SourcePath { get; set; }
        public string DestPath { get; set; }
        public int BufferSize { get; set; } = 2048;
    }

    public static class ArgumentsParser
    {
        public static CopyOptions Parse(string[] args)
        {
            var options = new CopyOptions();
            bool sourceFound = false;
            bool destFound = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--source" && i + 1 < args.Length)
                {
                    options.SourcePath = args[++i];
                    sourceFound = true;
                }
                else if (args[i] == "--dest" && i + 1 < args.Length)
                {
                    options.DestPath = args[++i];
                    destFound = true;
                }
                else if (args[i] == "--buffer" && i + 1 < args.Length)
                {
                    if (int.TryParse(args[++i], out int parsedBuffer))
                    {
                        options.BufferSize = parsedBuffer;
                    }
                }
            }

            if (!sourceFound || !destFound)
            {
                Console.WriteLine("Error: Missing arguments.");
                Console.WriteLine("Usage: Copytool --source <path> --dest <path> [--buffer <size>]");
                return null;
            }

            if (!File.Exists(options.SourcePath))
            {
                Console.WriteLine($"Error: Source file '{options.SourcePath}' does not exist.");
                return null;
            }

            return options;
        }
    }

    public class FileCopier
    {
        public void Copy(CopyOptions options)
        {
            PrepareDirectory(options.DestPath);

            long totalBytes = new FileInfo(options.SourcePath).Length;
            long totalBytesCopied = 0;
            byte[] buffer = new byte[options.BufferSize];
            int bytesRead;

            Stopwatch stopwatch = Stopwatch.StartNew();

            using (var sourceStream = new FileStream(options.SourcePath, FileMode.Open, FileAccess.Read))
            using (var destinationStream = new FileStream(options.DestPath, FileMode.Create, FileAccess.Write))
            {
                while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    destinationStream.Write(buffer, 0, bytesRead);
                    totalBytesCopied += bytesRead;

                    DisplayProgress(totalBytesCopied, totalBytes, stopwatch.Elapsed);
                }
            }

            stopwatch.Stop();
            Console.WriteLine("\nCopy process completed successfully.");
        }

        private void PrepareDirectory(string destPath)
        {
            string destDir = Path.GetDirectoryName(destPath);
            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }
        }

        private void DisplayProgress(long copied, long total, TimeSpan elapsed)
        {
            double percentage = total > 0 ? ((double)copied / total) * 100 : 100;
            string timeString = elapsed.ToString(@"hh\:mm\:ss");
            Console.Write($"\nProgress: {percentage:F2}% | Copied: {copied}/{total} bytes | Time: {timeString}");
        }
    }
}