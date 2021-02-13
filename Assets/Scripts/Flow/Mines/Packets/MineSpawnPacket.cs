using KarmanNet.Networking;
using System;
using UnityEngine;

public class MineSpawnPacket : Packet {
    private readonly Guid id;
    private readonly Vector2 position;
    private readonly float duration;

    public MineSpawnPacket(byte[] bytes) : base(bytes) {
        id = ReadGuid();
        position = ReadVector2();
        duration = ReadFloat();
    }

    public MineSpawnPacket(Guid id, Vector2 position, float duration) : base(
        Bytes.Pack(Bytes.Of(id), Bytes.Of(position), Bytes.Of(duration))
    ) {
        this.id = id;
        this.position = position;
        this.duration = duration;
    }

    public override bool IsValid() {
        return id != null && id != Guid.Empty
            && duration > 0f;
    }

    public Guid GetId() {
        return id;
    }

    public Vector2 GetPosition() {
        return position;
    }

    public float GetDuration() {
        return duration;
    }
}
