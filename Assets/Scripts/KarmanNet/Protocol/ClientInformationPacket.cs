using KarmanNet.Networking;
using System;

namespace KarmanNet.Protocol {
    public class ClientInformationPacket : Packet {
        private readonly Guid clientId;
        private readonly string clientName;
        private readonly Guid clientSecret;
        private readonly string serverPassword;

        public ClientInformationPacket(byte[] bytes) : base(bytes) {
            clientId = ReadGuid();
            clientName = ReadString();
            clientSecret = ReadGuid();
            serverPassword = ReadString();
        }

        public ClientInformationPacket(Guid clientId, string clientName, Guid clientSecret, string serverPassword) : base(
            Bytes.Pack(Bytes.Of(clientId), Bytes.Of(clientName), Bytes.Of(clientSecret), Bytes.Of(serverPassword))
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
