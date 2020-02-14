using System;
using System.IO;

namespace Blacksmith.ProjectCleaner
{
    internal static class Asserts
    {
        private class PrvAssertException : Exception
        {
            public PrvAssertException(string message)
                : base(message)
            {
            }
        }

        public static void check(bool condition)
        {
            prv_check(condition, "Assertion fail");
        }

        public static void check(bool condition, string message)
        {
            prv_check(condition, message);
        }

        public static void fail(string message)
        {
            prv_check(false, message);
        }

        public static void stringIsFilled(string text, string message)
        {
            prv_check(string.IsNullOrWhiteSpace(text) == false, message);
        }

        public static void itemIsNotNull(object item, string message)
        {
            prv_check(item != null, message);
        }

        public static void fileExists(string filefullPath, string message)
        {
            prv_check(File.Exists(filefullPath) == true, message);
        }

        public static void directoryExists(string directoryfullPath, string message)
        {
            prv_check(Directory.Exists(directoryfullPath) == true, message);
        }

        public static void isValidEnum<T>(object enumValue, string exceptionMessageFormat) where T : struct
        {
            prv_check(Enum.IsDefined(typeof(T), enumValue), exceptionMessageFormat);
        }

        private static void prv_check(bool condition, string message)
        {
            if (condition == false)
                throw new PrvAssertException(message);
        }
    };
}
