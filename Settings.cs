using System.Collections.Generic;

namespace Blacksmith.ProjectCleaner
{
    internal static class Settings
    {
        public static int DeleteRetries => 3;
        public static int MilisecondsOfPauseBetweenRetries => 1000;

        public static IEnumerable<string> DirectoryExclussionPatterns => new string[]
        {
            ".git",
        };

        public static IEnumerable<string> DirectoryPatterns => new string[]
        {
            "TestResults",
            "packages",
            ".vs",
            "node_modules",
            "dist",
        };

        public static IEnumerable<string> ProjectFilePatterns => new string[]
        {
            "*.vbproj",
            "*.csproj",
            "*.isproj",
            "package.json",
        };

        public static IEnumerable<string> ProjectSubDirectoryPatterns => new string[]
        {
            "bin",
            "obj",
        };

        public static IEnumerable<string> ProjectSubFilePatterns => new string[]
        {
            "*.user",
        };
    }
}
