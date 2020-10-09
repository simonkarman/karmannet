using Networking;
using UnityEngine;

public class ClientFlow : MonoBehaviour {
    public const string CONNECTION_STRING_PLAYER_PREFS_KEY = "connectionString";

    private string connectionString = "localhost";
    private MultiplayerClient client = null;
    private string username;

    public void Start() {
        connectionString = PlayerPrefs.GetString(CONNECTION_STRING_PLAYER_PREFS_KEY, "localhost");
        string buildId = Application.isEditor ? "<IN EDITOR>" : Application.buildGUID;
        Debug.Log(string.Format("Build id: {0}", buildId));

        username = string.Format("User-" + (Random.value * 10000).ToString("0000"));
        Debug.Log(string.Format("Username: {0}", username));

        client = new MultiplayerClient(ConnectionString.Parse(connectionString, ServerFlow.DEFAULT_PORT), PacketFactoryBuilder.FromAssemblies(), OnPacketReceived);
    }

    private float connectedTimeAtLastSend = 0f;
    public void Update() {
        if (client.IsConnected()) {
            float sendInterval = 5f;
            float connectedTime = Time.timeSinceLevelLoad;
            if (connectedTime > connectedTimeAtLastSend + sendInterval) {
                connectedTimeAtLastSend = Time.timeSinceLevelLoad;
                client.Send(new MessagePacket(string.Format(
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

    public void OnPacketReceived(Packet packet) {
        if (packet is MessagePacket messagePacket) {
            Debug.Log(string.Format("Server says: {0}", messagePacket.GetMessage()));

        } else if (packet is RequestUsernamePacket requestUsernamePacket) {
            ProvideUsernamePacket provideUsernamePacket = new ProvideUsernamePacket(requestUsernamePacket.GetSecret(), username);
            client.Send(provideUsernamePacket);
        }
    }
}
