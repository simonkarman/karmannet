using KarmanProtocol;
using System.Collections.Generic;
using UnityEngine;

public class MineReplicator : MonoBehaviour {
    [SerializeField]
    private ClientFlow clientFlow = default;
    [SerializeField]
    private GameObject minePrefab = default;

    private KarmanClient karmanClient;
    private readonly List<MineData> mines = new List<MineData>();

    protected void Start() {
        karmanClient = clientFlow.GetKarmanClient();
        karmanClient.OnPacketReceivedCallback += OnPacketReceived;
        karmanClient.OnLeftCallback += OnLeft;
    }

    private void OnPacketReceived(Networking.Packet packet) {
        if (packet is MineSpawnPacket mineSpawnPacket) {
            OnMineSpawnPacketReceived(mineSpawnPacket);
        }
    }

    private void OnMineSpawnPacketReceived(MineSpawnPacket packet) {
        Debug.Log("Received a MineSpawnPacket:" + packet.GetId());
        new MineData(packet.GetId(), packet.GetPosition(), packet.GetDuration(), Instantiate(minePrefab, transform));
    }

    private void OnLeft(string reason) {
        foreach (Mine mine in transform.GetComponentsInChildren<Mine>()) {
            Destroy(mine.gameObject);
        }
    }
}