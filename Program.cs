using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Blacksmith.ProjectCleaner
{
    class Program
    {
        private const int DELETE_RETRIES = 13;

        private static readonly IEnumerable<string> directoryExclussionPatterns = new string[]
        {
            ".git",
        };

        private static readonly IEnumerable<string> directoryPatterns = new string[]
        {
            "TestResults",
            "packages",
            ".vs",
            "node_modules",
            "dist",
        };

        private static readonly IEnumerable<string> projectFilePatterns = new string[]
        {
            "*.vbproj",
            "*.csproj",
            "*.isproj",
            "package.json",
        };

        private static readonly IEnumerable<string> projectSubDirectoryPatterns = new string[]
        {
            "bin",
            "obj",
        };

        private static readonly IEnumerable<string> projectSubFilePatterns = new string[]
        {
            "*.user",
        };

        private static int prv_deleteDirectory(DirectoryInfo dir, string subDirName)
        {
            DirectoryInfo subDir;

            Asserts.itemIsNotNull(dir, $"Parameter '{nameof(dir)}' cannot be null.");
            Asserts.stringIsFilled(subDirName, $"Parameter '{nameof(subDirName)}' cannot be empty.");

            subDir = dir
                .EnumerateDirectories(subDirName, SearchOption.TopDirectoryOnly)
                .SingleOrDefault();

            if (subDir != null)
                return prv_deleteEntry(subDir);
            else
                return 0;
        }

        private static int prv_deleteEntry(FileSystemInfo entry)
        {
            int errorsResult = 0;

            for (int i = 1; i <= DELETE_RETRIES; ++i)
            {
                try
                {
                    Console.Write($"Deleting {entry.FullName}... ");

                    if (entry is DirectoryInfo)
                        (entry as DirectoryInfo).Delete(true);
                    else
                        entry.Delete();

                    Console.WriteLine($"OK");
                    break;
                }
                catch (Exception ex)
                {
                    ++errorsResult;
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine($"Retry {i} of {DELETE_RETRIES}");
                    Console.ResetColor();
                    Thread.Sleep(1000);
                }
            }

            return errorsResult;
        }

        private static int prv_clean(DirectoryInfo currentDir)
        {
            int errorsResult;
            IEnumerable<FileInfo> entries;

            Asserts.check(currentDir.Exists, $"Directory '{currentDir}' does not exist.");

            errorsResult = 0;

            errorsResult += directoryPatterns
                .Where(pattern => directoryExclussionPatterns.Contains(pattern) == false)
                .Sum(pattern => prv_deleteDirectory(currentDir, pattern));

            entries = projectFilePatterns
                .SelectMany(pattern => currentDir.GetFiles(pattern));

            if (entries.Any())
            {
                errorsResult += projectSubDirectoryPatterns
                    .Where(pattern => directoryExclussionPatterns.Contains(pattern) == false)
                    .Sum(pattern => prv_deleteDirectory(currentDir, pattern));

                currentDir.Refresh();

                errorsResult += projectSubFilePatterns
                    .SelectMany(pattern => currentDir.GetFiles(pattern))
                    .Where(entry => directoryExclussionPatterns.Contains(entry.Name) == false)
                    .Sum(file => prv_deleteEntry(file));

                currentDir.Refresh();
            }

            errorsResult += currentDir
                .EnumerateDirectories()
                .Where(subDir => directoryExclussionPatterns.Contains(subDir.Name) == false)
                .Sum(dir => prv_clean(dir));

            return errorsResult;
        }

        static void Main(string[] args)
        {
            DirectoryInfo dir;
            int errors;

            Console.WriteLine("Cleaning projects...");
            dir = new DirectoryInfo(Environment.CurrentDirectory);

            for (int i = 1; i <= DELETE_RETRIES; ++i)
            {
                try
                {
                    errors = prv_clean(dir);
                    if (errors > 0)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"The were {errors} errors! Retrying {i} of {DELETE_RETRIES}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Finished!");
                        Console.ResetColor();
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"ERROR: {ex.Message}");
                    Console.WriteLine($"Retry {i} of {DELETE_RETRIES}");
                    Console.ResetColor();
                    System.Threading.Thread.Sleep(1000);
                }
            }

            Console.ReadLine();
        }
    }
}
