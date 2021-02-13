using KarmanNet.Networking;
using System;

public class CharacterDestroyPacket : Packet {
    private readonly Guid id;

    public CharacterDestroyPacket(byte[] bytes) : base(bytes) {
        id = ReadGuid();
    }

    public CharacterDestroyPacket(Guid id) : base(Bytes.Of(id)) {
        this.id = id;
    }

    public override bool IsValid() {
        return id != null && id != Guid.Empty;
    }

    public Guid GetId() {
        return id;
    }
}
