using KarmanNet.Networking;
using System;

namespace KarmanNet.Karmax {
    public class MutationPacket : Packet {
        private readonly Guid id;
        private readonly Guid requester;
        private readonly string fragmentId;
        private readonly byte[] payload;

        public MutationPacket(byte[] bytes) : base(bytes) {
            id = ReadGuid();
            requester = ReadGuid();
            fragmentId = ReadString();
            payload = ReadRestBytes();
        }

        public MutationPacket(Guid id, Guid requester, string fragmentId, byte[] payload) : base(
            Bytes.Pack(Bytes.Of(id), Bytes.Of(requester), Bytes.Of(fragmentId), payload)
        ) {
            this.id = id;
            this.requester = requester;
            this.fragmentId = fragmentId;
            this.payload = payload;
        }

        public Guid GetId() {
            return id;
        }

        public Guid GetRequester() {
            return requester;
        }

        public string GetFragmentId() {
            return fragmentId;
        }

        public byte[] GetPayload() {
            return payload;
        }

        public override bool IsValid() {
            return id != Guid.Empty && fragmentId != null && fragmentId.Length > 0 && payload != null && payload.Length >= 4;
        }
    }
}