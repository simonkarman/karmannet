using KarmanProtocol;
using UnityEngine;

public class ClientFlow : MonoBehaviour {
    public const string CONNECTION_STRING_PLAYER_PREFS_KEY = "client-flow:connection-string";

    [SerializeField]
    private bool sendConnectionMessages = false;
    [SerializeField]
    private float sendConnectionMessagesInterval = 5f;

    private float connectedTimeAtLastSend = 0f;
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

    public void Update() {
        if (karmanClient.IsConnected() && sendConnectionMessages) {
            float connectedTime = Time.timeSinceLevelLoad;
            if (connectedTime > connectedTimeAtLastSend + sendConnectionMessagesInterval) {
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
