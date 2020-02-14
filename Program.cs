using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Blacksmith.ProjectCleaner
{
    class Program
    {
        static void Main(string[] args)
        {
            DirectoryInfo dir;
            bool retry;

            Console.WriteLine("Cleaning projects...");
            dir = new DirectoryInfo(Environment.CurrentDirectory);
            retry = true;

            for (int i = 1; retry && i <= Settings.DeleteRetries; ++i)
            {
                int errors;

                try
                {
                    errors = prv_clean(dir);
                    retry = 0 < errors;

                    if (retry)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"The were {errors} errors! Retrying {i} of {Settings.DeleteRetries}");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Finished!");
                        Console.ResetColor();
                    }
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Error.WriteLine($"ERROR: {ex.Message}");
                    Console.Error.WriteLine($"Retry {i} of {Settings.DeleteRetries}");
                    Console.ResetColor();
                    Thread.Sleep(Settings.MilisecondsOfPauseBetweenRetries);
                }
            }

            Console.ReadLine();
        }

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

            try
            {
                Console.Write($"Deleting {entry.FullName}... ");

                prv_clearReadOnlyFlag(entry);

                if (entry is DirectoryInfo)
                    (entry as DirectoryInfo).Delete(true);
                else
                    entry.Delete();

                Console.WriteLine($"OK");
            }
            catch (Exception ex)
            {
                ++errorsResult;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine($"ERROR: {ex.Message}");
                Console.ResetColor();
                Thread.Sleep(Settings.MilisecondsOfPauseBetweenRetries / 4);
            }

            return errorsResult;
        }

        private static void prv_clearReadOnlyFlag(FileSystemInfo entry)
        {
            DirectoryInfo directory;

            if (entry.Attributes.HasFlag(FileAttributes.ReadOnly))
                entry.Attributes &= ~FileAttributes.ReadOnly;

            directory = entry as DirectoryInfo;

            if (directory != null)
                foreach (FileSystemInfo item in directory.EnumerateFileSystemInfos())
                    prv_clearReadOnlyFlag(item);
        }

        private static int prv_clean(DirectoryInfo currentDir)
        {
            int errorsResult;
            bool hasFilesForToBeDeleted;

            Asserts.check(currentDir.Exists, $"Directory '{currentDir}' does not exist.");

            errorsResult = 0;

            errorsResult += Settings
                .DirectoryPatterns
                .Where(pattern => false == Settings.DirectoryExclussionPatterns.Contains(pattern))
                .Sum(pattern => prv_deleteDirectory(currentDir, pattern));

            hasFilesForToBeDeleted = Settings
                .ProjectFilePatterns
                .SelectMany(currentDir.GetFiles)
                .Any();

            if (hasFilesForToBeDeleted)
            {
                errorsResult += Settings
                    .ProjectSubDirectoryPatterns
                    .Where(pattern => false == Settings.DirectoryExclussionPatterns.Contains(pattern))
                    .Sum(pattern => prv_deleteDirectory(currentDir, pattern));

                currentDir.Refresh();

                errorsResult += Settings
                    .ProjectSubFilePatterns
                    .SelectMany(pattern => currentDir.GetFiles(pattern))
                    .Where(entry => false == Settings.DirectoryExclussionPatterns.Contains(entry.Name))
                    .Sum(prv_deleteEntry);

                currentDir.Refresh();
            }

            errorsResult += currentDir
                .EnumerateDirectories()
                .Where(subDir => false == Settings.DirectoryExclussionPatterns.Contains(subDir.Name))
                .Sum(prv_clean);

            return errorsResult;
        }

    }
}
