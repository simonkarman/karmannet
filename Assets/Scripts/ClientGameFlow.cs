using UnityEngine;

public class ClientGameFlow : MonoBehaviour {
    [SerializeField]
    private string connectionString = "localhost";
    [SerializeField]
    private float messageSendInternal = 10f;
    [SerializeField]
    private float stayConnectedDuration = 60f;

    private AsynchronousClient client = null;
    private float timeSinceLastMessage = 0f;
    private bool welcomeMessageSend = false;
    private string username;

    public void Start() {
        string buildId = Application.isEditor ? "<IN EDITOR>" : Application.buildGUID;
        Debug.Log(string.Format("Build id: {0}", buildId));

        username = string.Format("User-" + (Random.value * 10000).ToString("0000"));
        client = new AsynchronousClient(connectionString, username, MessageCallback);
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            foreach (ScreenLog screenLog in FindObjectsOfType<ScreenLog>()) {
                screenLog.ToggleVisibility();
            }
        }

        if (client.Status == AsynchronousClientStatus.CONNECTED) {
            DoConnectedLogic();
        }
    }

    private void DoConnectedLogic() {
        if (!welcomeMessageSend) {
            welcomeMessageSend = true;
            client.Send(string.Format("Hello, server! My username is {0}.", username));
        }

        if (client.RealtimeSinceConnectionEstablished > stayConnectedDuration) {
            client.Disconnect();
        }

        timeSinceLastMessage += Time.deltaTime;
        if (timeSinceLastMessage >= messageSendInternal) {
            timeSinceLastMessage -= messageSendInternal;
            client.Send(string.Format("{0} has been around for {1} second(s).", client.username, client.RealtimeSinceConnectionEstablished));
        }
    }

    public void MessageCallback(string message) {
        new GameObject(message).transform.parent = transform;
    }
}
