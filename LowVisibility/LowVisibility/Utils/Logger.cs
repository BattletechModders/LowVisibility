using System;
using System.IO;

namespace LowVisibility {
    public class Logger {
        private static StreamWriter LogStream;

        public Logger(string modDir, string logName) {
            string logFile = Path.Combine(modDir, $"{logName}.log");
            if (File.Exists(logFile)) {
                File.Delete(logFile);
            }

            LogStream = File.AppendText(logFile);
            LogStream.AutoFlush = true;

        }

        public void LogIfDebug(string message) { if (LowVisibility.Config.Debug) { Log(message); } }
        public void LogIfTrace(string message) { if (LowVisibility.Config.Trace) { Log(message); } }

        public void Log(string message) {
            string now = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.CultureInfo.InvariantCulture);
            LogStream.WriteLine($"{now} - {message}");
        }

    }
}
