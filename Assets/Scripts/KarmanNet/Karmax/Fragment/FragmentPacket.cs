using KarmanNet.Networking;

namespace KarmanNet.Karmax {
    internal class FragmentPacket : Packet {
        private readonly byte[] key;
        private readonly byte[] payload;

        public FragmentPacket(byte[] bytes) : base(bytes) {
            int keyLength = ReadInt();
            key = ReadByteArray(keyLength);
            payload = ReadRestAsByteArray();
        }

        public FragmentPacket(byte[] key, byte[] payload) : base(Bytes.Pack(Bytes.Of(key.Length), key, payload)) {
            this.key = key;
            this.payload = payload;
        }

        public override bool IsValid() {
            return key != null && key.Length > 0 && payload != null && payload.Length >= 4;
        }

        public byte[] GetKey() {
            return key;
        }

        public byte[] GetPayload() {
            return payload;
        }
    }
}