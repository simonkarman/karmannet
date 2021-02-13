namespace KarmanNet.Logging {
    public interface ILogAppender {
        LogLevel GetLogLevel();
        void Append(LogLevel logLevel, LogMetaData logMetaData, string message, params object[] args);
    }
}
