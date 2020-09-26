using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScreenLog : MonoBehaviour {
    [SerializeField]
    private Text headerText = null;
    [SerializeField]
    private ScreenLogLine linePrefab = null;
    [SerializeField]
    private Transform linesParent = null;
    [SerializeField]
    private int maxNumberOfLines = 20;

    private bool visible = true;
    private int currentIndex = 0;
    private ScreenLogLine[] lines;

    protected void Awake() {
        headerText.text = string.Format("~ {0} scene of {1} [{2}] by {3}", SceneManager.GetActiveScene().name, Application.productName, Application.version, Application.companyName);
        ThreadManager.Activate();
        lines = new ScreenLogLine[maxNumberOfLines];
        Application.logMessageReceivedThreaded += HandleLog;
        Debug.Log(string.Format("Build id: {0}", Application.buildGUID));
    }

    protected void OnDestroy() {
        Application.logMessageReceivedThreaded -= HandleLog;
    }

    private void HandleLog(string log, string stackTrace, LogType type) {
        ThreadManager.ExecuteOnMainThread(() => {
            ScreenLogLine screenLogLine = Instantiate(linePrefab.gameObject, linesParent).GetComponent<ScreenLogLine>();
            screenLogLine.transform.SetAsFirstSibling();
            screenLogLine.Set(log, stackTrace, type);

            if (lines[currentIndex] != null) {
                Destroy(lines[currentIndex].gameObject);
            }
            lines[currentIndex] = screenLogLine;
            currentIndex += 1;
            currentIndex %= maxNumberOfLines;
        });
    }

    protected void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            visible = !visible;
            foreach (var line in lines) {
                if (line == null) {
                    continue;
                }

                line.gameObject.SetActive(visible);
            }
        }
    }
}
