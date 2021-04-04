using KarmanNet.Networking;
using System;

namespace KarmanNet.Karmax {
    public class MutationPacket : Packet {
        private readonly Guid id;
        private readonly Guid requester;
        private readonly byte[] key;
        private readonly byte[] payload;

        public MutationPacket(byte[] bytes) : base(bytes) {
            id = ReadGuid();
            requester = ReadGuid();
            int fragmentIdLength = ReadInt();
            key = ReadByteArray(fragmentIdLength);
            payload = ReadRestAsByteArray();
        }

        public MutationPacket(Guid id, Guid requester, byte[] key, byte[] payload) : base(
            Bytes.Pack(Bytes.Of(id), Bytes.Of(requester), Bytes.Of(key.Length), key, payload)
        ) {
            this.id = id;
            this.requester = requester;
            this.key = key;
            this.payload = payload;
        }

        public Guid GetId() {
            return id;
        }

        public Guid GetRequester() {
            return requester;
        }

        public byte[] GetKey() {
            return key;
        }

        public byte[] GetPayload() {
            return payload;
        }

        public override bool IsValid() {
            return id != Guid.Empty && key != null && key.Length > 0 && payload != null && payload.Length >= 4;
        }
    }
}