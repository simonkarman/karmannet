using Networking;
using System;

namespace KarmanProtocol {
    public class ServerInformationPacket : Packet {
        private readonly Guid serverId;
        private readonly string serverName;
        private readonly Guid gameId;
        private readonly string gameVersion;
        private readonly string karmanProtocolVersion;

        public ServerInformationPacket(byte[] bytes) : base(bytes) {
            byte[][] split = Bytes.Split(bytes);
            serverId = Bytes.GetGuid(split[0]);
            serverName = Bytes.GetString(split[1]);
            gameId = Bytes.GetGuid(split[2]);
            gameVersion = Bytes.GetString(split[3]);
            karmanProtocolVersion = Bytes.GetString(split[4]);
        }

        public ServerInformationPacket(Guid serverId, string serverName, Guid gameId, string gameVersion, string karmanProtocolVersion) : base(
            Bytes.Merge(Bytes.Of(serverId), Bytes.Of(serverName), Bytes.Of(gameId), Bytes.Of(gameVersion), Bytes.Of(karmanProtocolVersion))
        ) {
            this.serverId = serverId;
            this.serverName = serverName;
            this.gameId = gameId;
            this.gameVersion = gameVersion;
            this.karmanProtocolVersion = karmanProtocolVersion;
        }

        public override bool IsValid() {
            return serverId != null && serverId != Guid.Empty
                && serverName != null && serverName.Length > 2
                && gameId != null && gameId != Guid.Empty
                && gameVersion != null && gameVersion.Length > 0
                && karmanProtocolVersion != null & karmanProtocolVersion.Length > 2;
        }

        public Guid GetServerId() {
            return serverId;
        }

        public string GetServerName() {
            return serverName;
        }

        public Guid GetGameId() {
            return gameId;
        }

        public string GetGameVersion() {
            return gameVersion;
        }

        public string GetKarmanProtocolVersion() {
            return karmanProtocolVersion;
        }
    }
}
