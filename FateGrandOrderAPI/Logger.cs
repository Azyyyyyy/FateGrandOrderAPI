using System;
using System.IO;
using System.Diagnostics;

namespace FateGrandOrderApi.Logging
{
    internal class Logger
    {
        public static void LogConsole(Exception e, string LogMessage, string AdditionalData = "N/A", bool ToThrow = false)
        {
            var consoleColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($"[{nameof(FateGrandOrderApi)}]: ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"{LogMessage}\r\nException StackTrace:\r\n{e.StackTrace}\r\nException Message:\r\n{e.Message}");
            if(!string.IsNullOrWhiteSpace(AdditionalData))
                Console.WriteLine($"Additional Data:\r\n{AdditionalData}");
            Console.ForegroundColor = consoleColor;
            if (ToThrow)
                throw e;
        }

        public static void LogDebugger(Exception e, string LogMessage, string AdditionalData = "N/A", bool ToThrow = false)
        {
            if (Debugger.IsAttached)
            {
                Debug.WriteLine($"[{nameof(FateGrandOrderApi)}]: {LogMessage}\r\nException StackTrace:\r\n{e.StackTrace}\r\nException Message:\r\n{e.Message}");
                if (!string.IsNullOrWhiteSpace(AdditionalData))
                    Debug.WriteLine($"Additional Data:\r\n{AdditionalData}");
                if (ToThrow)
                    throw e;
            }
            else
            {
                LogConsole(e, LogMessage, AdditionalData, ToThrow);
            }
        }

        public static void LogFile(Exception e, string LogMessage, string AdditionalData = "N/A", bool ToThrow = false)
        {
            string FileContent = $"[{nameof(FateGrandOrderApi)}]: {LogMessage}\r\nException StackTrace:\r\n{e.StackTrace}\r\nException Message:\r\n{e.Message}Additional Data:\r\n{AdditionalData}";
            if (!Directory.Exists($"FateGrandOrderAPI"))
                Directory.CreateDirectory($"FateGrandOrderAPI");
            File.AppendAllText(Path.Combine($"FateGrandOrderAPI", "Log.txt"),FileContent);
            if (ToThrow)
                throw e;
        }

        public static void LogAll(Exception e, string LogMessage, string AdditionalData = "N/A", bool ToThrow = false)
        {
            LogConsole(e, LogMessage, AdditionalData, false);
            if (!string.IsNullOrWhiteSpace(AdditionalData))
                LogDebugger(e, LogMessage, AdditionalData, false);
            else
                LogDebugger(e, LogMessage, AdditionalData, false);
            LogFile(e, LogMessage);
            if (ToThrow)
                throw e;
        }
    }
}
