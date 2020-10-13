using Networking;
using System;

namespace KarmanProtocol {
    public class ClientInformationPacket : Packet {
        private readonly Guid clientId;
        private readonly Guid clientSecret;

        public ClientInformationPacket(byte[] bytes) : base(bytes) {
            byte[][] split = ByteHelper.Split(bytes);
            clientId = new Guid(split[0]);
            clientId = new Guid(split[1]);
        }

        public ClientInformationPacket(Guid clientId, Guid clientSecret) : base(ByteHelper.Merge(clientId.ToByteArray(), clientSecret.ToByteArray())) {
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
