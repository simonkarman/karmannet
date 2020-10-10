using Networking;
using System;
using UnityEngine;

public class KarmanClient {

    private readonly string connectionString;
    private readonly Guid clientId;

    private Client client;
    private string clientName;

    public KarmanClient(string connectionString, string clientName) {
        this.connectionString = connectionString;
        this.clientName = clientName;
        clientId = Guid.NewGuid();
        Debug.Log(string.Format("KarmanClient: Created a new KarmanClient with connectionString={0}, clientId={1}, clientName={2}", connectionString, clientId, clientName));

        TryConnect();
        // TODO: on disconnect, try and connect again
        // TODO: if client does not receive a ServerInformationPacket within X seconds, then disconnect
    }

    private void TryConnect() {
        client = new Client(ConnectionString.Parse(connectionString, ServerFlow.DEFAULT_PORT), OnPacketReceived);
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
            Debug.LogWarning(string.Format("KarmanClient: Did not handle a received packet that is of type {0}", packet.GetType().Name));
        }
    }

    public bool IsConnected() {
        // TODO: use on connected callback for this
        return client.Status == ConnectionStatus.CONNECTED;
    }

    public Guid GetClientId() {
        return clientId;
    }

    public void SetClientName(string clientName) {
        // TODO: send client name change packet
        this.clientName = clientName;
    }

    public string GetClientName() {
        return clientName;
    }

    public void Send(Packet packet) {
        client.Send(packet);
    }

    public void Leave() {
        if (client.Status == ConnectionStatus.CONNECTED) {
            client.Send(new LeavePacket());
            client.Disconnect();
        }
    }
}
