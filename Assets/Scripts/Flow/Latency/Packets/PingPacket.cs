using KarmanNet.Networking;
using System;

namespace KarmanNet.Protocol {
    public class PingPacket : Packet {

        private readonly Guid pingId;

        public PingPacket(byte[] bytes) : base(bytes) {
            pingId = ReadGuid();
        }

        public PingPacket(Guid pingId) : base(Bytes.Of(pingId)) {
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
