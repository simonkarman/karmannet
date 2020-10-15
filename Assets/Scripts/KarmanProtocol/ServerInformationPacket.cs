using Networking;
using System;

namespace KarmanProtocol {
    public class ServerInformationPacket : Packet {
        private readonly Guid serverId;
        private readonly string protocolVersion;

        public ServerInformationPacket(byte[] bytes) : base(bytes) {
            serverId = ReadGuid();
            protocolVersion = ReadString();
        }

        public ServerInformationPacket(Guid serverId, string protocolVersion) : base(
            Bytes.Pack(Bytes.Of(serverId), Bytes.Of(protocolVersion))
        ) {
            this.serverId = serverId;
            this.protocolVersion = protocolVersion;
        }

        public override void Validate() { }

        public string GetProtocolVersion() {
            return protocolVersion;
        }

        public Guid GetServerId() {
            return serverId;
        }
    }
}
