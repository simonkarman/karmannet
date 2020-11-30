using KarmanProtocol;
using Networking;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Logger = Logging.Logger;

public class PingMoment {
    private readonly Guid pingId;
    private readonly float moment;
    private int numberOfPingsInProgress = 0;

    public PingMoment() {
        pingId = Guid.NewGuid();
        moment = Time.realtimeSinceStartup;
    }

    public PingPacket GetPingPacket() {
        numberOfPingsInProgress++;
        return new PingPacket(pingId);
    }

    public bool ResolveOne(out int latency) {
        latency = Mathf.CeilToInt((Time.realtimeSinceStartup - moment) * 1000);
        return --numberOfPingsInProgress <= 0;
    }

    public Guid GetPingId() {
        return pingId;
    }

    public int GetNumberOfPingsInProgress() {
        return numberOfPingsInProgress;
    }
}

public class ClientLatencyData {
    private bool connected = false;
    private readonly LinkedList<int> latencyHistory = new LinkedList<int>();
    private readonly int maxLatencyHistorySize;

    public ClientLatencyData(int maxLatencyHistorySize) {
        this.maxLatencyHistorySize = maxLatencyHistorySize;
    }

    public void SetConnected(bool connected) {
        this.connected = connected;
    }

    public bool GetConnected() {
        return connected;
    }

    public void AddLatency(int latency) {
        latencyHistory.AddLast(latency);
        if (latencyHistory.Count > maxLatencyHistorySize) {
            latencyHistory.RemoveFirst();
        }
    }

    public int GetMaxLatency() {
        return latencyHistory.Max();
    }
}

public class LatencyOracle : MonoBehaviour {
    private readonly static Logger log = Logger.For<LatencyOracle>();

    [SerializeField]
    private ServerFlow serverFlow = default;
    [SerializeField]
    private float pingInterval = 1f;
    [SerializeField]
    private int maxLatencyHistorySize = 10;

    private KarmanServer karmanServer;
    private readonly Dictionary<Guid, PingMoment> pingMoments = new Dictionary<Guid, PingMoment>();
    private readonly Dictionary<Guid, ClientLatencyData> latencyDataPerClient = new Dictionary<Guid, ClientLatencyData>();
    private float nextPingMoment;

    public Action<Guid, int> OnClientAverageLatencyUpdatedCallback;

    protected void Start() {
        karmanServer = serverFlow.GetKarmanServer();
        karmanServer.OnClientJoinedCallback += OnClientJoined;
        karmanServer.OnClientConnectedCallback += OnClientConnected;
        karmanServer.OnClientDisconnectedCallback += OnClientDisconnected;
        karmanServer.OnClientLeftCallback += OnClientLeft;
        karmanServer.OnClientPacketReceivedCallback += OnClientPacketReceived;
    }

    protected void FixedUpdate() {
        if (nextPingMoment <= Time.realtimeSinceStartup) {
            nextPingMoment += pingInterval;
            PingMoment pingMoment = new PingMoment();
            pingMoments.Add(pingMoment.GetPingId(), pingMoment);
            foreach (var clientKvp in latencyDataPerClient) {
                Guid clientId = clientKvp.Key;
                ClientLatencyData clientData = clientKvp.Value;
                if (clientData.GetConnected()) {
                    karmanServer.Send(clientId, pingMoment.GetPingPacket());
                }
            }
            log.Trace("Send ping packet with ping id {0} to {1} client(s)", pingMoment.GetPingId(), pingMoment.GetNumberOfPingsInProgress());
        }
    }

    private void OnClientJoined(Guid clientId) {
        latencyDataPerClient.Add(clientId, new ClientLatencyData(maxLatencyHistorySize));
    }

    private void OnClientConnected(Guid clientId) {
        latencyDataPerClient[clientId].SetConnected(true);
    }

    private void OnClientDisconnected(Guid clientId) {
        latencyDataPerClient[clientId].SetConnected(false);
    }

    private void OnClientLeft(Guid clientId, string reason) {
        latencyDataPerClient.Remove(clientId);
    }

    private void OnClientPacketReceived(Guid clientId, Packet packet) {
        if (packet is PingResponsePacket pingResponsePacket) {
            Guid pingId = pingResponsePacket.GetPingId();
            if (pingMoments[pingId].ResolveOne(out int latency)) {
                pingMoments.Remove(pingId);
            }
            latencyDataPerClient[clientId].AddLatency(latency);
            log.Trace("Client {0} has a latency with the server of {1} milliseconds.", clientId, latency);
            OnClientAverageLatencyUpdatedCallback(clientId, latency);
        }
    }
}