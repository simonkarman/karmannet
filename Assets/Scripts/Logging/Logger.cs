using System;
using System.Collections.Generic;
using System.Linq;

namespace Logging {
    public class Logger {
        private static readonly List<ILogAppender> logAppenders = new List<ILogAppender>();

        private readonly Type loggerType;

        public static void ClearAppenders() {
            logAppenders.Clear();
        }

        public static void AddAppender(ILogAppender appender) {
            logAppenders.Add(appender);
        }

        public Logger(Type loggerType) {
            this.loggerType = loggerType;
        }

        public static Logger For<T>() {
            return new Logger(typeof(T));
        }

        public void Log(LogLevel logLevel, string message, params object[] args) {
            foreach (var appender in logAppenders.Where(app => app.GetLogLevel() <= logLevel)) {
                try {
                    appender.Append(logLevel, new LogMetaData(loggerType), message, args);
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

        public void Error(Exception ex) {
            Log(LogLevel.ERROR, "{0} {1}", ex.GetType().Name, ex.Message);
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
                Error(ex);
                return messageObject;
            } else {
                return Exit(LogLevel.ERROR, messageObject);
            }
        }
    }
}
