using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    public class Logging
    {
        // Method to log messages
        public static void LogMessage(List<LogItem> logItems, string LogFilePath )
        {
            string LogFile = System.IO.Path.Combine(LogFilePath, DateTime.Now.ToString("yyyy-MM-dd") + ".log");

            // Ensure the log directory exists
            if (!System.IO.Directory.Exists(LogFilePath))
            {
                System.IO.Directory.CreateDirectory(LogFilePath);
            }


            // Append Item to log file
            using (var writer = new System.IO.StreamWriter(LogFile, true))
            {
                foreach (LogItem logItem in logItems)
                {
                    writer.WriteLine($"{logItem.Timestamp:yyyy-MM-dd HH:mm:ss} [{logItem.Level}] {logItem.Source}: {logItem.Message}");
                }
            }
        }
    }


    public class LogItem
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string Level { get; set; } = "Info";  // e.g., Info, Warning, Error
        public string Source { get; set; } = "General"; // e.g., Class or method name

        public LogItem(DateTime timestamp, string message, string level, string source)
        {
            Timestamp = timestamp;
            Message = message;
            Level = level;
            Source = source;
        }
    }





}
