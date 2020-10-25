using Networking;
using System;

namespace KarmanProtocol {
    public class PingPacket : Packet {

        private readonly Guid pingId;

        public PingPacket(byte[] bytes) : base(bytes) {
            pingId = ReadGuid();
        }

        public PingPacket(Guid pingId) : base(Bytes.Of(pingId)) {
            this.pingId = pingId;
        }

        public override void Validate() { }

        public Guid GetPingId() {
            return pingId;
        }
    }
}
