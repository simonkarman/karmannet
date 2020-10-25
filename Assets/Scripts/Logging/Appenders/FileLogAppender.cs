using System.Collections.Generic;

namespace Logging {
    public class LogFileAppender : ILogAppender {
        private readonly LogLevel logLevel;
        private readonly string fileName;
        private readonly List<string> lines = new List<string>();

        private System.DateTime lastFlushDateTime = System.DateTime.MinValue;

        public LogFileAppender(string fileName, LogLevel logLevel) {
            this.logLevel = logLevel;
            this.fileName = fileName;
        }

        public void Append(LogLevel logLevel, LogMetaData logMetaData, string message, params object[] args) {
            string line = string.Format("[{0}] {1} {2}: {3}", logMetaData.GetTimestamp().ToString("yyyy-MM-dd-HH\\:mm\\:ss"), logLevel, logMetaData.GetClassName(), string.Format(message, args));
            lock (lines) {
                lines.Add(line);
            }
            if (lines.Count > 99 || System.DateTime.UtcNow > lastFlushDateTime.AddSeconds(2)) {
                Flush();
            }
        }

        public LogLevel GetLogLevel() {
            return logLevel;
        }

        public void Flush() {
            lock (lines) {
                System.IO.File.AppendAllLines(fileName, lines);
                lines.Clear();
            }
            lastFlushDateTime = System.DateTime.UtcNow;
        }
    }
}
