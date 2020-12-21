using Networking;
using System;
using UnityEngine;

public class CharacterSpawnPacket : Packet {
    private readonly Guid id;
    private readonly Guid clientId;
    private readonly Vector2 position;
    private readonly Color color;

    public CharacterSpawnPacket(byte[] bytes): base(bytes) {
        id = ReadGuid();
        clientId = ReadGuid();
        position = ReadVector2();
        color = ReadColor();
    }

    public CharacterSpawnPacket(Guid id, Guid clientId, Vector2 position, Color color): base(
        Bytes.Pack(Bytes.Of(id), Bytes.Of(clientId), Bytes.Of(position), Bytes.Of(color))
    ) {
        this.id = id;
        this.clientId = clientId;
        this.position = position;
        this.color = color;
    }

    public override bool IsValid() {
        return id != null && id != Guid.Empty
            && clientId != null && clientId != Guid.Empty;
    }

    public Guid GetId() {
        return id;
    }

    public Guid GetClientId() {
        return clientId;
    }

    public Vector2 GetPosition() {
        return position;
    }

    public Color GetColor() {
        return color;
    }
}
