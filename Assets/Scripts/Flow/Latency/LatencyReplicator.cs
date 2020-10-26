using KarmanProtocol;
using Networking;
using UnityEngine;

public class LatencyReplicator : MonoBehaviour {
    [SerializeField]
    private ClientFlow clientFlow = default;

    private KarmanClient karmanClient;

    protected void Start() {
        karmanClient = clientFlow.GetKarmanClient();
        karmanClient.OnPacketReceivedCallback += OnPacketReceived;
    }

    private void OnPacketReceived(Packet packet) {
        if (packet is PingPacket pingPacket) {
            PingResponsePacket pingResponsePacket = new PingResponsePacket(pingPacket.GetPingId());
            karmanClient.Send(pingResponsePacket);
        }
    }
}
