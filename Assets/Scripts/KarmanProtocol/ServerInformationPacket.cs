using System;
using System.Text;

namespace KarmanProtocol {
    public class ServerInformationPacket : Networking.Packet {

        private readonly Guid serverId;
        private readonly string protocolVersion;
        private readonly string serverName;

        public ServerInformationPacket(byte[] bytes) : base(bytes) {
            string[] splits = Encoding.ASCII.GetString(bytes).Split('|');
            serverId = Guid.Parse(splits[0]);
            protocolVersion = splits[1];
            serverName = splits[2];
        }

        public ServerInformationPacket(Guid serverId, string protocolVersion, string serverName) : base(
            Encoding.ASCII.GetBytes(string.Format("{0}|{1}|{2}", serverId, protocolVersion, serverName))
        ) {
            this.serverId = serverId;
            this.protocolVersion = protocolVersion;
            this.serverName = serverName;
        }

        public override void Validate() {
            if (protocolVersion.Contains("|")) {
                throw new InvalidOperationException("Cannot create a ServerInformationPacket with protocolVersion that contains a '|' character");
            }
            if (serverName.Contains("|")) {
                throw new InvalidOperationException("Cannot create a ServerInformationPacket with serverName that contains a '|' character");
            }
        }

        public string GetProtocolVersion() {
            return protocolVersion;
        }

        public Guid GetServerId() {
            return serverId;
        }

        public string GetServerName() {
            return serverName;
        }

    }
}
