using System;
using System.Text;

namespace KarmanProtocol {
    public class ServerInformationPacket : Networking.Packet {
        private readonly Guid serverId;
        private readonly string protocolVersion;

        public ServerInformationPacket(byte[] bytes) : base(bytes) {
            string[] splits = Encoding.ASCII.GetString(bytes).Split('|');
            if (splits.Length != 2) {
                throw new InvalidOperationException("Cannot create a ServerInformationPacket when the '|' character splits it anything other than exactly 2 groups");
            }
            serverId = Guid.Parse(splits[0]);
            protocolVersion = splits[1];
        }

        public ServerInformationPacket(Guid serverId, string protocolVersion) : base(
            Encoding.ASCII.GetBytes(string.Format("{0}|{1}", serverId, protocolVersion))
        ) {
            this.serverId = serverId;
            this.protocolVersion = protocolVersion;
        }

        public override void Validate() {
            if (protocolVersion.Contains("|")) {
                throw new InvalidOperationException("Cannot create a ServerInformationPacket with protocolVersion that contains a '|' character");
            }
        }

        public string GetProtocolVersion() {
            return protocolVersion;
        }

        public Guid GetServerId() {
            return serverId;
        }
    }
}
