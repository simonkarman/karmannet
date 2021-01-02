using Logging;
using UnityEngine;
using Logger = Logging.Logger;

public class LoggingConfiguration : MonoBehaviour {
    private static readonly Logger log = Logger.For<LoggingConfiguration>();
    private LogFileAppender logFileAppender;

    [SerializeField]
    private string logFileName = default;

    protected void Awake() {
        Logger.ClearAppenders();
        Logger.AddAppender(new UnityDebugAppender(LogLevel.Info));
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.LinuxPlayer) {
            logFileAppender = new LogFileAppender(string.Format("{0}/log-{1}-{2}.txt", Application.dataPath, logFileName, System.DateTime.UtcNow.ToString("yyyyMMdd-HHmmss")), LogLevel.Info);
            Logger.AddAppender(logFileAppender);
        }

        string buildId = Application.isEditor ? "<IN EDITOR>" : Application.buildGUID;
        log.Info(string.Format("Build id: {0}", buildId));
    }

    protected void OnDestroy() {
        if (logFileAppender != null) {
            logFileAppender.Flush();
        }
    }
}
