using System.Text;
using UnityEngine;
using Networking;

public class ClientFlow : MonoBehaviour {
    [SerializeField]
    private string connectionString = "localhost";
    [SerializeField]
    private float messageSendInterval = 10f;
    [SerializeField]
    private float stayConnectedDuration = 60f;

    private MultiplayerClient client = null;
    private float timeSinceLastMessage = 0f;
    private bool welcomeMessageSend = false;
    private string username;

    public void Start() {
        string buildId = Application.isEditor ? "<IN EDITOR>" : Application.buildGUID;
        Debug.Log(string.Format("Build id: {0}", buildId));

        username = string.Format("User-" + (Random.value * 10000).ToString("0000"));
        Debug.Log(string.Format("Username: {0}", username));

        client = new MultiplayerClient(ConnectionString.Parse(connectionString, ServerFlow.DEFAULT_PORT), OnFrameReceived);
    }

    public void OnDestroy() {
        if (client.Status == ConnectionStatus.CONNECTED) {
            client.Disconnect();
        }
    }

    public void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            foreach (ScreenLog screenLog in FindObjectsOfType<ScreenLog>()) {
                screenLog.ToggleVisibility();
            }
        }

        if (client.Status == ConnectionStatus.CONNECTED) {
            UpdateWhenConnected();
        }
    }

    private void UpdateWhenConnected() {
        if (!welcomeMessageSend) {
            welcomeMessageSend = true;
            client.Send(Encoding.ASCII.GetBytes(string.Format(
                "Hello, server! My username is {0}.",
                username
            )));
            client.Send(Encoding.ASCII.GetBytes("Hi."));
        }

        timeSinceLastMessage += Time.deltaTime;
        if (timeSinceLastMessage >= messageSendInterval) {
            timeSinceLastMessage -= messageSendInterval;
            client.Send(Encoding.ASCII.GetBytes(string.Format(
                "{0} has been around for {1} second(s).",
                username,
                client.RealtimeSinceConnectionEstablished
            )));
        }

        if (client.RealtimeSinceConnectionEstablished > stayConnectedDuration) {
            client.Disconnect();
        }
    }

    public void OnFrameReceived(byte[] frame) {
        Debug.Log(string.Format("Server says: {0}", Encoding.ASCII.GetString(frame)));
    }
}
