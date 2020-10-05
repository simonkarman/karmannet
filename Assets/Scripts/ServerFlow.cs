using Networking;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ServerFlow : MonoBehaviour {

    public class ServerFlowPlayer {
        private readonly ServerFlow serverFlow;
        private readonly Guid connectionId;
        private readonly Guid secret;
        private string username;
        private string lastReceivedMessage;

        public ServerFlowPlayer(ServerFlow serverFlow, Guid connectionId) {
            this.serverFlow = serverFlow;
            this.connectionId = connectionId;
            username = "...";
            secret = Guid.NewGuid();
        }

        public Guid GetConnectionId() {
            return connectionId;
        }

        public string GetName() {
            return username;
        }

        public void Kick() {
            MessagePacket messagePacket = new MessagePacket("You were kicked by the server!");
            serverFlow.server.Send(connectionId, messagePacket);
            serverFlow.server.Disconnect(connectionId);
        }

        public void SetLastReceivedMessage(string lastReceivedMessage) {
            this.lastReceivedMessage = lastReceivedMessage;
        }

        public string GetLastReceivedMessage() {
            return lastReceivedMessage;
        }

        public void SetUsername(string username) {
            this.username = username;
        }

        public Guid GetSecret() {
            return secret;
        }
    }

    public const int DEFAULT_PORT = 14641;
    private MultiplayerServer server;
    private readonly Dictionary<Guid, ServerFlowPlayer> players = new Dictionary<Guid, ServerFlowPlayer>();
    public Action<IReadOnlyList<ServerFlowPlayer>> onPlayersChanged;

    public void ScheduleShutdown() {
        StartCoroutine(DoScheduledShutdown());
    }

    private IEnumerator<YieldInstruction> DoScheduledShutdown() {
        int shutdownDelay = 5;
        MessagePacket messagePacket = new MessagePacket(string.Format("The server is shutting down in {0} seconds!", shutdownDelay));
        server.Broadcast(messagePacket);
        yield return new WaitForSeconds(shutdownDelay);
        server.Shutdown();
    }

    protected void Start() {
        server = new MultiplayerServer(DEFAULT_PORT, PacketFactoryBuilder.GetPacketFactory(), OnConnected, OnDisconnected, OnPacketReceived);
    }

    protected void OnDestroy() {
        if (server.Status == ServerStatus.RUNNING) {
            server.Shutdown();
        }
    }

    public ServerStatus GetServerStatus() {
        return server.Status;
    }

    private void OnConnected(Guid connectionId) {
        Debug.Log(string.Format("ServerFlow: Client {0} connected", connectionId));
        MessagePacket messagePacket = new MessagePacket(string.Format("Welcome {0}. You're now connected to the server!", connectionId));
        server.Send(connectionId, messagePacket);

        ServerFlowPlayer player = new ServerFlowPlayer(this, connectionId);
        players.Add(connectionId, player);

        RequestUsernamePacket requestUsernamePacket = new RequestUsernamePacket(player.GetSecret());
        server.Send(connectionId, requestUsernamePacket);

        onPlayersChanged(new List<ServerFlowPlayer>(players.Values));
    }

    private void OnDisconnected(Guid connectionId) {
        Debug.Log(string.Format("ServerFlow: Client {0} disconnected", connectionId));
        players.Remove(connectionId);
        onPlayersChanged(new List<ServerFlowPlayer>(players.Values));
    }

    private void OnPacketReceived(Guid connectionId, Packet packet) {
        if (!players.TryGetValue(connectionId, out ServerFlowPlayer player)) {
            throw new InvalidOperationException(string.Format("Cannot handle packet with connection {0} because that connection does not exist", connectionId));
        }

        if (packet is MessagePacket messagePacket) {
            Debug.Log(string.Format("ServerFlow: Client {0} says: {1}", connectionId, messagePacket.GetMessage()));
            player.SetLastReceivedMessage(messagePacket.GetMessage());
            onPlayersChanged(new List<ServerFlowPlayer>(players.Values));

        } else if (packet is ProvideUsernamePacket provideUsernamePacket) {
            if (provideUsernamePacket.GetSecret().Equals(player.GetSecret())) {
                player.SetUsername(provideUsernamePacket.GetUsername());
            } else {
                player.Kick();
            }
            onPlayersChanged(new List<ServerFlowPlayer>(players.Values));
        }
    }
}
