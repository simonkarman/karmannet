using KarmanProtocol;
using UnityEngine;

public class MineReplicator : MonoBehaviour {
    [SerializeField]
    private ClientFlow clientFlow = default;
    [SerializeField]
    private GameObject minePrefab = default;

    private KarmanClient karmanClient;

    protected void Start() {
        karmanClient = clientFlow.GetKarmanClient();
        karmanClient.OnPacketReceivedCallback += OnPacketReceived;
    }

    private void OnPacketReceived(Networking.Packet packet) {
        if (packet is MineSpawnPacket mineSpawnPacket) {
            OnMineSpawnPacketReceived(mineSpawnPacket);
        }
    }

    private void OnMineSpawnPacketReceived(MineSpawnPacket packet) {
        Debug.Log("Received a MineSpawnPacket:" + packet.GetId());
        new MineData(packet.GetId(), packet.GetPosition(), packet.GetDuration(), minePrefab);
    }

    private void OnLeft() {
        // TODO: Destroy still existing mines
    }
}