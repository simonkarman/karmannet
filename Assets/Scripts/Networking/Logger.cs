using System;
using System.Collections.Generic;
using System.Linq;

namespace Networking {
    public enum LogLevel {
        TRACE = 0,
        INFO = 1,
        WARNING = 2,
        ERROR = 3,
    }

    public class LogMetaData {
        private readonly string loggerName;
        private readonly DateTime timestamp;
        private readonly string stackTrace;

        public LogMetaData(string loggerName) {
            this.loggerName = loggerName;
            timestamp = DateTime.UtcNow;
            stackTrace = Environment.StackTrace;
        }

        public string GetClassName() {
            return loggerName;
        }

        public DateTime GetTimestamp() {
            return timestamp;
        }

        public string GetStackTrace() {
            return stackTrace;
        }
    }

    public interface IAppender {
        LogLevel GetLogLevel();
        void Append(LogLevel logLevel, LogMetaData logMetaData, string message, params object[] args);
    }

    public class Logger {
        private static List<IAppender> appenders = new List<IAppender>();

        private readonly string loggerName;

        public static void ClearAppenders() {
            appenders.Clear();
        }

        public static void AddAppender(IAppender appender) {
            appenders.Add(appender);
        }

        public Logger(string loggerName) {
            this.loggerName = loggerName;
        }

        public static Logger For<T>() {
            return new Logger(typeof(T).FullName);
        }

        public void Log(LogLevel logLevel, string message, params object[] args) {
            foreach (var appender in appenders.Where(app => app.GetLogLevel() <= logLevel)) {
                try {
                    appender.Append(logLevel, new LogMetaData(loggerName), message, args);
                } catch { }
            }
        }

        public void Trace(string message, params object[] args) {
            Log(LogLevel.TRACE, message, args);
        }

        public void Info(string message, params object[] args) {
            Log(LogLevel.INFO, message, args);
        }

        public void Warning(string message, params object[] args) {
            Log(LogLevel.WARNING, message, args);
        }

        public void Error(string message, params object[] args) {
            Log(LogLevel.ERROR, message, args);
        }

        public T Exit<T>(LogLevel logLevel, T messageObject) {
            Log(logLevel, "{0}", messageObject);
            return messageObject;
        }

        public T ExitTrace<T>(T messageObject) {
            return Exit(LogLevel.TRACE, messageObject);
        }

        public T ExitInfo<T>(T messageObject) {
            return Exit(LogLevel.INFO, messageObject);
        }

        public T ExitWarning<T>(T messageObject) {
            return Exit(LogLevel.WARNING, messageObject);
        }

        public T ExitError<T>(T messageObject) {
            if (messageObject is Exception ex) {
                Log(LogLevel.ERROR, ex.Message);
                return messageObject;
            } else {
                return Exit(LogLevel.ERROR, messageObject);
            }
        }
    }
}