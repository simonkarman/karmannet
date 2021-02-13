using KarmanNet.Protocol;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CharacterReplicator : MonoBehaviour {
    [SerializeField]
    private ClientFlow clientFlow = default;
    [SerializeField]
    private GameObject characterPrefab = default;

    private KarmanClient karmanClient;
    private readonly Dictionary<Guid, CharacterData> characters = new Dictionary<Guid, CharacterData>();

    protected void Start() {
        karmanClient = clientFlow.GetKarmanClient();
        karmanClient.OnPacketReceivedCallback += OnPacketReceived;
        karmanClient.OnLeftCallback += OnLeft;
    }

    private void OnPacketReceived(KarmanNet.Networking.Packet packet) {
        if (packet is CharacterSpawnPacket characterSpawnPacket) {
            OnCharacterSpawnPacketReceived(characterSpawnPacket);
        } else if (packet is CharacterDestroyPacket characterDestroyPacket) {
            OnCharacterDestroyPacketReceived(characterDestroyPacket);
        } else if (packet is CharacterUpdatePositionPacket characterUpdatePositionPacket) {
            OnCharacterUpdatePositionPacketReceived(characterUpdatePositionPacket);
        }
    }

    private void OnCharacterSpawnPacketReceived(CharacterSpawnPacket packet) {
        Debug.Log("Received a CharacterSpawnPacket:" + packet.GetId());
        GameObject instance = Instantiate(characterPrefab, transform);
        Character character = instance.GetComponent<Character>();
        Color color;
        if (karmanClient.id.Equals(packet.GetClientId())) {
            character.enabled = true;
            color = packet.GetColor();
        } else {
            Destroy(character.GetComponent<Rigidbody2D>());
            Destroy(character);
            color = packet.GetColor();
            color.a = 0.3f;
        }
        characters.Add(packet.GetId(), new CharacterData(
            packet.GetId(), packet.GetClientId(), packet.GetPosition(), color, instance
        ));
    }

    private void OnCharacterDestroyPacketReceived(CharacterDestroyPacket packet) {
        Debug.Log("Received a CharacterDestroyPacket:" + packet.GetId());
        characters[packet.GetId()].Destroy();
        characters.Remove(packet.GetId());
    }

    private void OnCharacterUpdatePositionPacketReceived(CharacterUpdatePositionPacket packet) {
        characters[packet.GetId()].SetPosition(packet.GetPosition());
    }

    private void OnLeft(string reason) {
        foreach (var character in characters.Values) {
            character.Destroy();
        }
        characters.Clear();
    }

    protected void FixedUpdate() {
        foreach (var character in characters.Values) {
            if (character.RequestPositionSyncCheck()) {
                CharacterUpdatePositionPacket characterUpdatePositionPacket = character.GetUpdatePositionPacket();
                karmanClient.Send(characterUpdatePositionPacket);
            }
        }
    }
}