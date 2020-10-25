using System;

namespace Logging {
    public class LogMetaData {
        private readonly Type loggerType;
        private readonly DateTime timestamp;
        private readonly string stackTrace;

        public LogMetaData(Type loggerType) {
            this.loggerType = loggerType;
            timestamp = DateTime.UtcNow;
            stackTrace = Environment.StackTrace;
        }

        public string GetClassName() {
            return loggerType.FullName;
        }

        public DateTime GetTimestamp() {
            return timestamp;
        }

        public string GetStackTrace() {
            return stackTrace;
        }
    }
}
