using Networking;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ServerFlow : MonoBehaviour {

    public class ServerFlowPlayer {
        private readonly ServerFlow serverFlow;
        private readonly Guid connectionId;
        private readonly string name;
        private string lastReceivedMessage;

        public ServerFlowPlayer(ServerFlow serverFlow, Guid connectionId, string name) {
            this.serverFlow = serverFlow;
            this.connectionId = connectionId;
            this.name = name;
        }


        public Guid GetConnectionId() {
            return connectionId;
        }

        public string GetName() {
            return name;
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
        players.Add(connectionId, new ServerFlowPlayer(this, connectionId, "Unknown Player"));
        onPlayersChanged(new List<ServerFlowPlayer>(players.Values));
    }

    private void OnDisconnected(Guid connectionId) {
        Debug.Log(string.Format("ServerFlow: Client {0} disconnected", connectionId));
        players.Remove(connectionId);
        onPlayersChanged(new List<ServerFlowPlayer>(players.Values));
    }

    private void OnPacketReceived(Guid connectionId, Packet packet) {
        if (packet is MessagePacket messagePacket) {
            Debug.Log(string.Format("ServerFlow: Client {0} says: {1}", connectionId, messagePacket.GetMessage()));
            if (players.TryGetValue(connectionId, out ServerFlowPlayer player)) {
                player.SetLastReceivedMessage(messagePacket.GetMessage());
                onPlayersChanged(new List<ServerFlowPlayer>(players.Values));
            } else {
                throw new InvalidOperationException(string.Format("Cannot set last received message of the player with connection {0} because that connection does not exist", connectionId));
            }
        }
    }
}
