using KarmanNet.Networking;

namespace KarmanNet.Protocol {
    public class ClientAcceptedPacket : Packet {
        public ClientAcceptedPacket(byte[] bytes) : base(bytes) {}
        public ClientAcceptedPacket() : base(Bytes.Empty) {}

        public override bool IsValid() {
            return true;
        }
    }
}
