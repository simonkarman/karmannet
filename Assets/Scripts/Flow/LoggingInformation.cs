using Networking;
using System.Collections.Generic;
using UnityEngine;

public class LoggingInformation : MonoBehaviour {
    private static readonly Networking.Logger log = Networking.Logger.For<LoggingInformation>();
    private readonly LogFileAppender logFileAppender = new LogFileAppender(string.Format("{0}-log.txt", System.DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")), LogLevel.INFO);

    private class UnityDebugAppender : IAppender {
        private readonly LogLevel logLevel;

        public UnityDebugAppender(LogLevel logLevel) {
            this.logLevel = logLevel;
        }

        public void Append(LogLevel logLevel, LogMetaData logMetaData, string message, params object[] args) {
            System.Action<string, object[]> unityLogFunction;
            switch (logLevel) {
            default:
                unityLogFunction = Debug.LogFormat;
                break;
            case LogLevel.WARNING:
                unityLogFunction = Debug.LogWarningFormat;
                break;
            case LogLevel.ERROR:
                unityLogFunction = Debug.LogErrorFormat;
                break;
            }
            unityLogFunction(string.Format("{0} {1}: {2}", logLevel, logMetaData.GetClassName(), message), args);
        }

        public LogLevel GetLogLevel() {
            return logLevel;
        }
    }

    private class LogFileAppender : IAppender {
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

    protected void Awake() {
        Networking.Logger.ClearAppenders();
        Networking.Logger.AddAppender(new UnityDebugAppender(LogLevel.INFO));
        if (!Application.isEditor && !Application.isMobilePlatform) {
            Networking.Logger.AddAppender(logFileAppender);
        }

        string buildId = Application.isEditor ? "<IN EDITOR>" : Application.buildGUID;
        log.Info(string.Format("Build id: {0}", buildId));
    }

    protected void OnDestroy() {
        logFileAppender.Flush();
    }
}
