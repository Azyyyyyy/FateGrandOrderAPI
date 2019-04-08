using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ApiTest")]
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

            if (e != null)
                Console.WriteLine($"{LogMessage}\r\nException StackTrace:\r\n{e.StackTrace}\r\nException Message:\r\n{e.Message}");
            else if (!string.IsNullOrWhiteSpace(LogMessage))
                Console.WriteLine(LogMessage);
            else
                Console.WriteLine("Something happened (No Exception or log message)");

            if(!string.IsNullOrWhiteSpace(AdditionalData))
                Console.WriteLine($"Additional Data:\r\n{AdditionalData}");

            Console.ForegroundColor = consoleColor;
            if (ToThrow)
            {
                if (e != null)
                    throw e;
                else if (!string.IsNullOrEmpty(LogMessage))
                    throw new Exception(LogMessage);
                else
                    throw new Exception($"[{nameof(FateGrandOrderApi)}]: Something happened (No Exception or log message)");
            }
        }

        public static void LogDebugger(Exception e, string LogMessage, string AdditionalData = "N/A", bool ToThrow = false)
        {
            if (Debugger.IsAttached)
            {
                if (e != null)
                    Debugger.Log(0, nameof(FateGrandOrderApi), $"{LogMessage}\r\nException StackTrace:\r\n{e.StackTrace}\r\nException Message:\r\n{e.Message}");
                else if (!string.IsNullOrWhiteSpace(LogMessage))
                    Debugger.Log(0, nameof(FateGrandOrderApi), LogMessage);
                else
                    Debugger.Log(0, nameof(FateGrandOrderApi), "Something happened (No Exception or log message)");

                if (!string.IsNullOrWhiteSpace(AdditionalData))
                    Debug.WriteLine($"Additional Data:\r\n{AdditionalData}");

                if (ToThrow)
                {
                    if (e != null)
                        throw e;
                    else if (!string.IsNullOrEmpty(LogMessage))
                        throw new Exception(LogMessage);
                    else
                        throw new Exception($"[{nameof(FateGrandOrderApi)}]: Something happened (No Exception or log message)");
                }
            }
            else
            {
                LogConsole(e, LogMessage, AdditionalData, ToThrow);
            }
        }

        public static void LogFile(Exception e, string LogMessage, string AdditionalData = "N/A", bool ToThrow = false)
        {
            string FileContent = null;
            if (e != null)
                FileContent = $"[{nameof(FateGrandOrderApi)}]: {LogMessage}\r\nException StackTrace:\r\n{e.StackTrace}\r\nException Message:\r\n{e.Message}Additional Data:\r\n{AdditionalData}";
            else if (!string.IsNullOrWhiteSpace(LogMessage))
                FileContent = $"[{nameof(FateGrandOrderApi)}]: {LogMessage}";
            else
                FileContent = $"[{nameof(FateGrandOrderApi)}]: Something happened (No Exception or log message)";

            File.AppendAllText("FateGrandOrderAPILog.txt", FileContent);
            if (ToThrow)
            {
                if (e != null)
                    throw e;
                else if (!string.IsNullOrEmpty(LogMessage))
                    throw new Exception(LogMessage);
                else
                    throw new Exception($"[{nameof(FateGrandOrderApi)}]: Something happened (No Exception or log message)");
            }
        }

        public static void LogAll(Exception e, string LogMessage, string AdditionalData = "N/A", bool ToThrow = false)
        {
            LogConsole(e, LogMessage, AdditionalData, false);
            LogDebugger(e, LogMessage, AdditionalData, false);
            LogFile(e, LogMessage);
            if (ToThrow) { return; }
            if (e != null)
                throw e;
                else if (!string.IsNullOrEmpty(LogMessage))
                    throw new Exception(LogMessage);
                else
                    throw new Exception($"[{nameof(FateGrandOrderApi)}]: Something happened (No Exception or log message)");
        }
    }
}
