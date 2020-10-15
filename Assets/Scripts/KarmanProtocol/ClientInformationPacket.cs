using Networking;
using System;

namespace KarmanProtocol {
    public class ClientInformationPacket : Packet {
        private readonly Guid clientId;
        private readonly Guid clientSecret;

        public ClientInformationPacket(byte[] bytes) : base(bytes) {
            clientId = ReadGuid();
            clientSecret = ReadGuid();
        }

        public ClientInformationPacket(Guid clientId, Guid clientSecret) : base(
            Bytes.Pack(Bytes.Of(clientId), Bytes.Of(clientSecret))
        ) {
            this.clientId = clientId;
            this.clientSecret = clientSecret;
        }

        public override void Validate() {
            if (clientId == Guid.Empty) {
                throw new InvalidOperationException("Cannot create a ClientInformationPacket with a clientId of Guid.Empty");
            }
        }

        public Guid GetClientId() {
            return clientId;
        }

        public Guid GetClientSecret() {
            return clientSecret;
        }
    }
}
