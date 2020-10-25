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

        public override void Validate() { }

        public Guid GetPingId() {
            return pingId;
        }
    }
}
