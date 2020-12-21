using Networking;
using System;

namespace KarmanProtocol {
    public class PingResponsePacket : Packet {

        private readonly Guid pingId;

        public PingResponsePacket(byte[] bytes) : base(bytes) {
            pingId = ReadGuid();
        }

        public PingResponsePacket(Guid pingId) : base(Bytes.Of(pingId)) {
            this.pingId = pingId;
        }

        public override bool IsValid() {
            return pingId != null && pingId != Guid.Empty;
        }

        public Guid GetPingId() {
            return pingId;
        }
    }
}
