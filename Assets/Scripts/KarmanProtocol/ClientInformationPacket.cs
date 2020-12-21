using Networking;
using System;

namespace KarmanProtocol {
    public class ClientInformationPacket : Packet {
        private readonly Guid clientId;
        private readonly string clientName;
        private readonly Guid clientSecret;
        private readonly string serverPassword;

        public ClientInformationPacket(byte[] bytes) : base(bytes) {
            byte[][] splits = Bytes.Split(bytes);
            clientId = Bytes.GetGuid(splits[0]);
            clientName = Bytes.GetString(splits[1]);
            clientSecret = Bytes.GetGuid(splits[2]);
            serverPassword = Bytes.GetString(splits[3]);
        }

        public ClientInformationPacket(Guid clientId, string clientName, Guid clientSecret, string serverPassword) : base(
            Bytes.Merge(Bytes.Of(clientId), Bytes.Of(clientName), Bytes.Of(clientSecret), Bytes.Of(serverPassword))
        ) {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
            this.serverPassword = serverPassword;
            this.clientName = clientName;
        }

        public override bool IsValid() {
            return clientId != null && clientId != Guid.Empty
                && clientName != null & clientName.Length > 2
                && clientSecret != null & clientSecret != Guid.Empty
                && serverPassword != null;
        }

        public Guid GetClientId() {
            return clientId;
        }

        public string GetClientName() {
            return clientName;
        }

        public Guid GetClientSecret() {
            return clientSecret;
        }

        public string GetServerPassword() {
            return serverPassword;
        }
    }
}
