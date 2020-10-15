using Networking;

namespace KarmanProtocol {
    public class MessagePacket : Packet {
        private readonly string message;

        public MessagePacket(byte[] bytes) : base(bytes) {
            message = ReadString();
        }

        public MessagePacket(string message) : base(
            Bytes.Of(message)
        ) {
            this.message = message;
        }

        public override void Validate() { }

        public string GetMessage() {
            return message;
        }
    }
}
