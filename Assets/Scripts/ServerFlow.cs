using KarmanProtocol;
using System;
using System.Collections.Generic;
using UnityEngine;

public class ServerFlow : MonoBehaviour {

    public const int DEFAULT_PORT = 14641;
    public const string protocolVersion = "0.0.1";

    [SerializeField]
    private string serverName = default;

    private KarmanServer server;

    public Action<IReadOnlyList<IConnectedKarmanClient>> OnClientsChanged;

    protected void Start() {
        server = new KarmanServer(DEFAULT_PORT, protocolVersion, serverName, OnClientsChanged);
    }

    protected void OnDestroy() {
        if (server.IsRunning()) {
            server.Shutdown();
        }
    }

    public bool IsServerRunning() {
        return server.IsRunning();
    }

    public Guid GetServerId() {
        return server.id;
    }

    public string GetServerName() {
        return server.name;
    }

    public string GetServerProtocolVersion() {
        return server.protocolVersion;
    }

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

    public void Kick(Guid clientId) {
        MessagePacket messagePacket = new MessagePacket("You were kicked by the server!");
        server.Send(clientId, messagePacket);
        server.Kick(clientId);
    }
}
