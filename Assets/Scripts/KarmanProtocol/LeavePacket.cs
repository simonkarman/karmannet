using Networking;

namespace KarmanProtocol {
    public class LeavePacket : Packet {
        private readonly string reason;

        public LeavePacket(byte[] bytes) : base(bytes) {
            reason = ReadRawString();
        }
        public LeavePacket(string reason) : base(Bytes.Of(reason, Bytes.StringMode.RAW)) {
            this.reason = reason;
        }

        public override bool IsValid() {
            return reason != null && reason.Length > 0;
        }

        public string GetReason() {
            return reason;
        }
    }
}
