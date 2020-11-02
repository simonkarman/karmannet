using Networking;
using System;

namespace KarmanProtocol {
    public class ServerInformationPacket : Packet {
        private readonly Guid serverId;
        private readonly Guid gameId;
        private readonly string protocolVersion;

        public ServerInformationPacket(byte[] bytes) : base(bytes) {
            serverId = ReadGuid();
            gameId = ReadGuid();
            protocolVersion = ReadString();
        }

        public ServerInformationPacket(Guid serverId, Guid gameId, string protocolVersion) : base(
            Bytes.Pack(Bytes.Of(serverId), Bytes.Of(gameId), Bytes.Of(protocolVersion))
        ) {
            this.serverId = serverId;
            this.gameId = gameId;
            this.protocolVersion = protocolVersion;
        }

        public override void Validate() { }

        public Guid GetServerId() {
            return serverId;
        }

        public Guid GetGameId() {
            return gameId;
        }

        public string GetProtocolVersion() {
            return protocolVersion;
        }
    }
}
