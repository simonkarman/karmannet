using KarmanNet.Networking;

namespace KarmanNet.Karmax {
    public class FragmentPacket : Packet {
        private readonly string id;
        private readonly byte[] payload;

        public FragmentPacket(byte[] bytes) : base(bytes) {
            id = ReadString();
            payload = ReadRestBytes();
        }

        public FragmentPacket(string id, byte[] payload) : base(Bytes.Pack(Bytes.Of(id), payload)) {
            this.id = id;
            this.payload = payload;
        }

        public override bool IsValid() {
            return id != null && id.Length > 0 && payload != null && payload.Length >= 4;
        }

        public string GetId() {
            return id;
        }

        public byte[] GetPayload() {
            return payload;
        }
    }
}