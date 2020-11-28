using Networking;

namespace KarmanProtocol {
    public class LeavePacket : Packet {
        private string reason;

        public LeavePacket(byte[] bytes) : base(bytes) {
            reason = ReadString();
        }
        public LeavePacket(string reason) : base(Bytes.Of(reason)) {
            this.reason = reason;
        }

        public override void Validate() { }

        public string GetReason() {
            return reason;
        }
    }
}
