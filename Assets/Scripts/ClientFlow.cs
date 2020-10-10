using Networking;
using System;
using UnityEngine;

public class ClientFlow : MonoBehaviour {
    public const string CONNECTION_STRING_PLAYER_PREFS_KEY = "connectionString";

    private string connectionString = "localhost";
    private Client client = null;
    private Guid clientId;
    private string clientName;

    public void Start() {
        connectionString = PlayerPrefs.GetString(CONNECTION_STRING_PLAYER_PREFS_KEY, "localhost");
        string buildId = Application.isEditor ? "<IN EDITOR>" : Application.buildGUID;
        Debug.Log(string.Format("Build id: {0}", buildId));

        clientId = Guid.NewGuid();
        clientName = string.Format("User-" + (UnityEngine.Random.value * 10000).ToString("0000"));
        Debug.Log(string.Format("ClientId: {0} and ClientName: {1}", clientId, clientName));

        client = new Client(ConnectionString.Parse(connectionString, ServerFlow.DEFAULT_PORT), OnPacketReceived);

        // TODO: if client does not receive a ServerInformationPacket within X seconds, then disconnect
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
                    clientName,
                    connectedTime.ToString("0")
                )));
            }
        }
    }

    public void OnDestroy() {
        if (client.Status == ConnectionStatus.CONNECTED) {
            client.Send(new LeavePacket());
            client.Disconnect();
        }
    }

    public void OnPacketReceived(Packet packet) {
        if (packet is MessagePacket messagePacket) {
            Debug.Log(string.Format("Server says: {0}", messagePacket.GetMessage()));

        } else if (packet is ServerInformationPacket serverInformationPacket) {
            Debug.Log(string.Format(
                "Server send its information serverId={0}, protocolVersion={1}, and serverName={2}",
                serverInformationPacket.GetServerId(), serverInformationPacket.GetProtocolVersion(), serverInformationPacket.GetServerName()
            ));
            if (ServerFlow.protocolVersion != serverInformationPacket.GetProtocolVersion()) {
                Debug.LogError(string.Format("Disconnecting from server since it uses a different protocol version {0} than the client {1}", ServerFlow.protocolVersion, serverInformationPacket.GetProtocolVersion()));
                client.Disconnect();
            } else {
                ClientInformationPacket provideUsernamePacket = new ClientInformationPacket(clientId, clientName);
                client.Send(provideUsernamePacket);
            }
        
        } else {
            Debug.Log(string.Format("ClientFlow: Received an unhandled packet of type {0}", packet.GetType().Name));
        }
    }
}
