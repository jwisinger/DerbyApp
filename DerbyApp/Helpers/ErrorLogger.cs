using System;
using System.IO;
using System.Reflection;

namespace DerbyApp.Helpers
{
    public class ErrorLogger
    {
        private static string _logFilePath;

        public static string LogFilePath
        {
            get => _logFilePath;
            set
            {
                _logFilePath = value;
                // Ensure the directory exists
                string directory = Path.GetDirectoryName(_logFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }

        // Static constructor to set the default log file path
        static ErrorLogger()
        {
            // Set log file name to be the name of the executing assembly with a .log extension
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{assemblyName}.log");
        }

        /// <summary>
        /// Logs an error message and exception details to a file.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="exception">The exception object (optional).</param>
        public static void LogError(string message, Exception exception = null)
        {
            string logEntry = FormatLogEntry(message, exception);

            try
            {
                // Append the log entry to the file. Create the file if it does not exist.
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                // Handle potential errors during file writing (e.g., file lock, permissions)
                Console.WriteLine($"Failed to write to log file: {ex.Message}");
            }
        }

        /// <summary>
        /// Formats the log entry with a timestamp and exception details.
        /// </summary>
        private static string FormatLogEntry(string message, Exception exception)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string formattedEntry = $"[{timestamp}] ERROR: {message}";

            if (exception != null)
            {
                // Append exception details if provided
                formattedEntry += Environment.NewLine + "Exception Details:" + Environment.NewLine;
                formattedEntry += exception.ToString(); // exception.ToString() provides full details including stack trace
            }

            return formattedEntry;
        }
    }
}
