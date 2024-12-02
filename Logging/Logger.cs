using System;
using System.IO;
using System.Threading;

namespace Techolics_.Logging
{
    public sealed class Logger
    {
        private static readonly Lazy<Logger> lazyInstance = new Lazy<Logger>(() => new Logger());
        private static readonly object lockObj = new object();
        private readonly string logFilePath;

        private Logger()
        {
            // Set the log file path
            string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
            Directory.CreateDirectory(logDirectory); // Ensure the directory exists

            string logFileName = "Techolics_Log.txt"; // Single file for all logs
            logFilePath = Path.Combine(logDirectory, logFileName);

            // Optionally, write a header to the log file
            WriteLog("==== Application Started ====");
        }

        public static Logger Instance => lazyInstance.Value;

        public void WriteLog(string message,
            [System.Runtime.CompilerServices.CallerFilePath] string filePath = "",
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0)
        {
            lock (lockObj)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(logFilePath, true))
                    {
                        string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | {message}";
                        if (!string.IsNullOrEmpty(filePath))
                        {
                            logEntry += $" | File: {Path.GetFileName(filePath)}";
                        }
                        if (!string.IsNullOrEmpty(memberName))
                        {
                            logEntry += $" | Member: {memberName}";
                        }
                        if (lineNumber != 0)
                        {
                            logEntry += $" | Line: {lineNumber}";
                        }
                        writer.WriteLine(logEntry);
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions related to logging itself
                    Console.Error.WriteLine($"Logging failed: {ex.Message}");
                }
            }
        }
    }
}
