using Networking;
using System;
using UnityEngine;

public class CharacterUpdatePositionPacket : Packet {
    private Guid id;
    private Vector2 position;

    public CharacterUpdatePositionPacket(byte[] bytes): base(bytes) {
        id = ReadGuid();
        position = ReadVector2();
    }

    public CharacterUpdatePositionPacket(Guid id, Vector2 position): base(
        Bytes.Pack(Bytes.Of(id), Bytes.Of(position))
    ) {
        this.id = id;
        this.position = position;
    }

    public override bool IsValid() {
        return id != null && id != Guid.Empty;
    }

    public Guid GetId() {
        return id;
    }

    public Vector2 GetPosition() {
        return position;
    }
}
