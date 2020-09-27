using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScreenLog : MonoBehaviour {
    [SerializeField]
    private GameObject screenLogRootParent = null;
    [SerializeField]
    private Text headerText = null;
    [SerializeField]
    private ScreenLogLine linePrefab = null;
    [SerializeField]
    private Transform linesParent = null;
    [SerializeField]
    private int maxNumberOfLines = 20;

    private bool visible = true;
    private int currentIndex = -1;
    private ScreenLogLine[] lines;
    private int duplicationCounter;
    private string latestLogMessage;
    private LogType latestLogType;

    protected void Awake() {
        headerText.text = string.Format("~ {0} scene of {1} [{2}] by {3}", SceneManager.GetActiveScene().name, Application.productName, Application.version, Application.companyName);
        ThreadManager.Activate();
        lines = new ScreenLogLine[maxNumberOfLines];
        Application.logMessageReceivedThreaded += HandleLog;
    }

    protected void OnDestroy() {
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    private void HandleLog(string logMessage, string stackTrace, LogType logType) {
        ThreadManager.ExecuteOnMainThread(() => {
            if (logMessage.Equals(latestLogMessage) && logType.Equals(latestLogType)) {
                duplicationCounter++;
                lines[currentIndex].Set(string.Format("{0}x {1}", duplicationCounter, logMessage), logType);
                return;
            }
            currentIndex += 1;
            currentIndex %= maxNumberOfLines;

            duplicationCounter = 0;
            latestLogMessage = logMessage;
            latestLogType = logType;

            ScreenLogLine screenLogLine = lines[currentIndex];
            if (screenLogLine == null) {
                screenLogLine = Instantiate(linePrefab.gameObject, linesParent).GetComponent<ScreenLogLine>();
                lines[currentIndex] = screenLogLine;
            }
            screenLogLine.transform.SetAsLastSibling();
            screenLogLine.Set(logMessage, logType);
        });
    }

    public void ToggleVisibility() {
        visible = !visible;
        screenLogRootParent.SetActive(visible);
    }
}
