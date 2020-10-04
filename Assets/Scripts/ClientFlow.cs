using Networking;
using System.Text;
using UnityEngine;

public class ClientFlow : MonoBehaviour {
    [SerializeField]
    private string connectionString = "localhost";

    private MultiplayerClient client = null;
    private string username;

    public void Start() {
        string buildId = Application.isEditor ? "<IN EDITOR>" : Application.buildGUID;
        Debug.Log(string.Format("Build id: {0}", buildId));

        username = string.Format("User-" + (Random.value * 10000).ToString("0000"));
        Debug.Log(string.Format("Username: {0}", username));

        client = new MultiplayerClient(ConnectionString.Parse(connectionString, ServerFlow.DEFAULT_PORT), OnFrameReceived);
    }

    private float connectedTimeAtLastSend = 0f;
    public void Update() {
        if (client.IsConnected()) {
            float sendInterval = 1f;
            float connectedTime = client.RealtimeSinceConnectionEstablished;
            if (connectedTime > connectedTimeAtLastSend + sendInterval) {
                connectedTimeAtLastSend = connectedTime;
                client.Send(Encoding.ASCII.GetBytes(string.Format(
                    "{0} has been connected for {1} second(s).",
                    username,
                    connectedTime.ToString("0")
                )));
            }
        }
    }

    public void OnDestroy() {
        if (client.Status == ConnectionStatus.CONNECTED) {
            client.Disconnect();
        }
    }

    public void OnFrameReceived(byte[] frame) {
        Debug.Log(string.Format("Server says: {0}", Encoding.ASCII.GetString(frame)));
    }
}
