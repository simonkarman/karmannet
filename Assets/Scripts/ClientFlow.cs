using KarmanProtocol;
using UnityEngine;

public class ClientFlow : MonoBehaviour {
    public const string CONNECTION_STRING_PLAYER_PREFS_KEY = "connectionString";

    private KarmanClient karmanClient;

    public void Start() {
        string connectionString = PlayerPrefs.GetString(CONNECTION_STRING_PLAYER_PREFS_KEY, "localhost");
        string clientName = string.Format("User-" + (Random.value * 10000).ToString("0000"));
        karmanClient = new KarmanClient(connectionString, ServerFlow.DEFAULT_SERVER_PORT, clientName);
    }

    private float connectedTimeAtLastSend = 0f;
    public void Update() {
        if (karmanClient.IsConnected()) {
            float sendInterval = 5f;
            float connectedTime = Time.timeSinceLevelLoad;
            if (connectedTime > connectedTimeAtLastSend + sendInterval) {
                connectedTimeAtLastSend = Time.timeSinceLevelLoad;
                karmanClient.Send(new MessagePacket(string.Format(
                    "{0} has been connected for {1} second(s).",
                    karmanClient.GetClientName(),
                    connectedTime.ToString("0")
                )));
            }
        }
    }

    public void OnDestroy() {
        karmanClient.Leave();
    }
}
