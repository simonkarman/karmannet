using UnityEngine;

namespace KarmanNet.Logging {
    public class UnityDebugAppender : ILogAppender {
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
            case LogLevel.Warning:
                unityLogFunction = Debug.LogWarningFormat;
                break;
            case LogLevel.Error:
                unityLogFunction = Debug.LogErrorFormat;
                break;
            }
            unityLogFunction(string.Format("{0} {1}: {2}", logLevel, logMetaData.GetClassName(), message), args);
        }

        public LogLevel GetLogLevel() {
            return logLevel;
        }
    }
}
