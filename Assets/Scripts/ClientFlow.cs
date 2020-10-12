using KarmanProtocol;
using UnityEngine;

public class ClientFlow : MonoBehaviour {
    public const string CONNECTION_STRING_PLAYER_PREFS_KEY = "connectionString";

    private KarmanClient karmanClient;

    protected void Awake() {
        karmanClient = new KarmanClient();
    }

    public void Start() {
        string connectionString = PlayerPrefs.GetString(CONNECTION_STRING_PLAYER_PREFS_KEY, "localhost");
        karmanClient.Start(connectionString, ServerFlow.DEFAULT_SERVER_PORT);
    }

    public void OnDestroy() {
        if (karmanClient.IsConnected()) {
            karmanClient.Leave();
        }
    }

    public KarmanClient GetKarmanClient() {
        return karmanClient;
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
                    karmanClient.id,
                    connectedTime.ToString("0")
                )));
            }
        }
    }
}
