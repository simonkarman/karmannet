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

        public override bool IsValid() {
            return message != null && message.Length > 0;
        }

        public string GetMessage() {
            return message;
        }
    }
}
